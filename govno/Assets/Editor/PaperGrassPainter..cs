using UnityEngine;
using UnityEditor;

public class PaperGrassPainter : EditorWindow
{
    private GameObject grassPrefab;
    private float brushRadius = 2f;
    private float spawnDensity = 0.3f; // Вероятность спавна за шаг
    private float minScale = 0.8f;
    private float maxScale = 1.3f;
    private bool isPainting = false;

    [MenuItem("Tools/Paper Grass Painter")]
    public static void ShowWindow()
    {
        GetWindow<PaperGrassPainter>("Grass Painter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Настройки бумажной травы", EditorStyles.boldLabel);

        grassPrefab = (GameObject)EditorGUILayout.ObjectField("Префаб травы", grassPrefab, typeof(GameObject), false);
        brushRadius = EditorGUILayout.Slider("Радиус кисти", brushRadius, 0.5f, 10f);
        spawnDensity = EditorGUILayout.Slider("Плотность", spawnDensity, 0.05f, 1f);

        GUILayout.Label("Случайный масштаб", EditorStyles.miniLabel);
        EditorGUILayout.BeginHorizontal();
        minScale = EditorGUILayout.FloatField("Мин.", minScale);
        maxScale = EditorGUILayout.FloatField("Макс.", maxScale);
        EditorGUILayout.EndHorizontal();

        if (isPainting)
        {
            if (GUILayout.Button("Выключить кисть (STOP)")) isPainting = false;
        }
        else
        {
            if (GUILayout.Button("Включить кисть (Зажмите Ctrl во вьюпорте)")) isPainting = true;
        }

        if (isPainting)
        {
            EditorGUILayout.HelpBox("Зажмите CTRL и водите мышкой по 3D-моделям в Scene View для рисования.", MessageType.Info);
        }
    }

    private void OnEnable() => SceneView.duringSceneGui += OnSceneGUI;
    private void OnDisable() => SceneView.duringSceneGui -= OnSceneGUI;

    private void OnSceneGUI(SceneView sceneView)
    {
        if (!isPainting || grassPrefab == null) return;

        Event e = Event.current;

        // Работаем только если зажат Ctrl
        if (e.control && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown) && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            RaycastHit hit;

            // Пускаем луч в сцену, игнорируя уже созданную траву
            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {
                // Рисуем круг кисти в редакторе
                Handles.color = new Color(0, 1, 0, 0.2f);
                Handles.DrawSolidDisc(hit.point, hit.normal, brushRadius);

                // Рандомайзер плотности, чтобы не спавнить миллион объектов в одну секунду
                if (Random.value < spawnDensity)
                {
                    // Смещение точки спавна случайным образом внутри радиуса кисти
                    Vector2 randomCircle = Random.insideUnitCircle * brushRadius;
                    Vector3 spawnOffset = new Vector3(randomCircle.x, 0, randomCircle.y);

                    // Корректируем точку спавна под рельеф модели
                    Vector3 targetPos = hit.point + spawnOffset;
                    Ray checkRay = new Ray(targetPos + hit.normal * 2f, -hit.normal);
                    RaycastHit checkHit;

                    if (Physics.Raycast(checkRay, out checkHit, 5f))
                    {
                        // Проверяем, что не пытаемся вырастить траву на самой траве
                        if (checkHit.collider.gameObject.name.Contains(grassPrefab.name)) return;

                        // Спавним префаб через PrefabUtility, чтобы сохранить связь с префабом
                        GameObject newGrass = (GameObject)PrefabUtility.InstantiatePrefab(grassPrefab);
                        newGrass.transform.position = checkHit.point;

                        // 1. Сначала ставим траву вертикально (относительно мира)
                        newGrass.transform.up = Vector3.up;

                        // 2. Наклоняем её в сторону нормали полигона 3D-модели
                        // Quaternion.FromToRotation вычисляет правильный угол наклона
                        newGrass.transform.rotation = Quaternion.FromToRotation(Vector3.up, checkHit.normal);

                        // 3. Добавляем случайный поворот вокруг СОБСТВЕННОЙ вертикальной оси (ось Y)
                        newGrass.transform.Rotate(Vector3.up, Random.Range(0f, 360f), Space.Self);

                        // Рандомный масштаб для эффекта «живой» бумаги
                        float randomScale = Random.Range(minScale, maxScale);
                        newGrass.transform.localScale = Vector3.one * randomScale;

                        // Привязываем к родителю, если кликнули на конкретную модель, чтобы сцена оставалась чистой
                        newGrass.transform.SetParent(checkHit.collider.transform, true);

                        // Регистрируем операцию для возможности отмены через Ctrl+Z
                        Undo.RegisterCreatedObjectUndo(newGrass, "Paint Paper Grass");
                    }
                }

                // Пожираем событие мыши, чтобы не выделять объекты в редакторе во время рисования
                e.Use();
            }
        }

        // Принудительно обновляем SceneView, чтобы круг кисти не лагал
        if (isPainting) sceneView.Repaint();
    }
}