// LOCAL TOOL - never part of the submission repo. Editor scripting is only taught in week 13.
// Menu: Tools > Assignment 01 > Build Scene
// Rebuilds Assets/Scenes/Assignment_01.unity and Assets/Prefabs/Coin.prefab from scratch every time it runs.
// Builds the original assignment AND the lecturer-approved extensions (see CLAUDE.md, "Extensions").

using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class Assignment01Builder
{
    // ---------------------------------------------------------------- paths
    const string ScenePath = "Assets/Scenes/Assignment_01.unity";
    const string PrefabFolder = "Assets/Prefabs";
    const string CoinPrefabPath = "Assets/Prefabs/Coin.prefab";
    const string ArtFolder = "Assets/Art/Generated";
    const string CapsulePath = ArtFolder + "/Capsule.png";
    const string ChipPath = ArtFolder + "/Chip.png";
    const string ShadowPath = ArtFolder + "/Shadow.png";
    const string SquarePath = ArtFolder + "/Square.png";
    const string FeltPath = ArtFolder + "/Felt.png";
    const string ChessboardPath = ArtFolder + "/Chessboard.png";
    const string FeltCustomPath = "Assets/Art/Felt_Custom.png";
    const string TmpSettingsPath = "Assets/TextMesh Pro/Resources/TMP Settings.asset";

    // ---------------------------------------------------------------- palette (CLAUDE.md)
    static readonly Color32 FeltColor = new Color32(0x1E, 0x5A, 0x3C, 255);
    static readonly Color32 RimColor = new Color32(0x5B, 0x3A, 0x24, 255);
    static readonly Color32 PlayerColor = new Color32(0xFF, 0xFF, 0xFF, 255);
    static readonly Color32 EnemyColor = new Color32(0xD9, 0x3A, 0x3A, 255);
    static readonly Color32 KnightColor = new Color32(0x20, 0x20, 0x20, 255);
    static readonly Color32 KnightRimColor = new Color32(0xBD, 0xBD, 0xBD, 255); // thin outline so it reads on dark felt
    static readonly Color32 MarkerColor = new Color32(0xFF, 0xD8, 0x4D, 110);    // semi-transparent landing marker
    static readonly Color32 ChipOrange = new Color32(0xF2, 0x8C, 0x28, 255);
    static readonly Color32 ChipGreen = new Color32(0x3C, 0xB3, 0x71, 255);
    static readonly Color32 ChipBlue = new Color32(0x3A, 0x7B, 0xD5, 255);
    static readonly Color32 ShadowColor = new Color32(0, 0, 0, 128); // black at 50% alpha
    static readonly Color32 White = new Color32(255, 255, 255, 255);

    // ---------------------------------------------------------------- sorting orders (CLAUDE.md)
    const int OrderBackground = -10;
    const int OrderBoard = -9;
    const int OrderRim = -5;
    const int OrderShadow = 5;
    const int OrderMarker = 6;
    const int OrderCoin = 8;
    const int OrderPawn = 10;
    const int OrderKnight = 11;

    // ---------------------------------------------------------------- sizes (world units)
    // Capsule and chip textures are 256 pixels per unit: 128x256 px = 0.5 x 1 units, 128x128 px = 0.5 x 0.5 units.
    const float PawnWidth = 0.5f;
    const float PawnHeight = 1f;
    const float ChipDiameter = 0.5f;
    const float WallThickness = 0.6f;
    const float WallCenterX = 8.5f;
    const float WallCenterY = 4.7f;
    const float FeltWorldWidth = 20f; // felt covers the 16:9 view of an orthographic camera with size 5 (17.8 x 10)
    const float KnightScale = 1.2f;   // a bit bigger than the pawns

    // Shadows: ellipse sprite is 0.5 x 0.25 units, these scales make it clearly visible.
    static readonly Vector3 PawnShadowOffset = new Vector3(0.06f, -0.38f, 0f);
    static readonly Vector3 PawnShadowScale = new Vector3(1.7f, 1.3f, 1f);
    static readonly Vector3 CoinShadowOffset = new Vector3(0.04f, -0.06f, 0f);
    static readonly Vector3 CoinShadowScale = new Vector3(1.5f, 3f, 1f);

    // Chessboard: 18 x 10 cells of 1 unit. The playable table is the central 16 x 8 cells; the rim covers most of the outer ring.
    const int BoardCellsX = 18;
    const int BoardCellsY = 10;
    const int BoardPixelsPerCell = 64;

    [MenuItem("Tools/Assignment 01/Build Scene")]
    public static void BuildScene()
    {
        if (EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Assignment01Builder: stop Play mode first, then run the builder again.");
            return;
        }

        // Rebuilding replaces the scene file, so give the user the chance to save whatever is open.
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return;
        }

        // a. tags
        EnsureTag("Enemy");
        EnsureTag("Coin");

        // b. sprites
        EnsureFolder("Assets/Art");
        EnsureFolder(ArtFolder);
        EnsureFolder(PrefabFolder);
        EnsureFolder("Assets/Scenes");

        bool useCustomFelt = File.Exists(ToFullPath(FeltCustomPath));
        string feltPath = useCustomFelt ? FeltCustomPath : FeltPath;

        if (!File.Exists(ToFullPath(CapsulePath))) SaveTexture(CapsulePath, MakeCapsule(128, 256));
        if (!File.Exists(ToFullPath(ChipPath))) SaveTexture(ChipPath, MakeChip(128));
        if (!File.Exists(ToFullPath(ShadowPath))) SaveTexture(ShadowPath, MakeShadow(128, 64));
        if (!File.Exists(ToFullPath(SquarePath))) SaveTexture(SquarePath, MakeSquare(4));
        if (!File.Exists(ToFullPath(ChessboardPath))) SaveTexture(ChessboardPath, MakeChessboard());
        if (!useCustomFelt && !File.Exists(ToFullPath(FeltPath))) SaveTexture(FeltPath, MakeFelt(1024, 576));

        float feltPpu = useCustomFelt ? GetTextureWidth(FeltCustomPath) / FeltWorldWidth : 1024f / FeltWorldWidth;
        Sprite capsuleSprite = ImportSprite(CapsulePath, 256f);
        Sprite chipSprite = ImportSprite(ChipPath, 256f);
        Sprite shadowSprite = ImportSprite(ShadowPath, 256f);
        Sprite squareSprite = ImportSprite(SquarePath, 4f);
        Sprite boardSprite = ImportSprite(ChessboardPath, BoardPixelsPerCell);
        Sprite feltSprite = ImportSprite(feltPath, feltPpu);

        // c. scene
        Scene scene = CreateScene();

        // d. camera
        SetupCamera();

        // e. background, chessboard and walls
        CreateSpriteObject("Background", feltSprite, White, OrderBackground, Vector3.zero, Vector3.one);
        CreateSpriteObject("Chessboard", boardSprite, White, OrderBoard, Vector3.zero, Vector3.one);
        CreateWalls(squareSprite);

        // f. player and enemy
        GameObject player = CreatePawn("Player", "Player", new Vector3(-4f, 0f, 0f), PlayerColor,
            capsuleSprite, shadowSprite, false);
        GameObject enemy = CreatePawn("Enemy", "Enemy", new Vector3(5f, 3f, 0f), EnemyColor,
            capsuleSprite, shadowSprite, true);

        // g. coin prefab
        GameObject coinPrefab = CreateCoinPrefab(chipSprite, shadowSprite);

        // h. managers and wiring
        GameObject managerObject = new GameObject("GameManager");
        GameManager gameManager = managerObject.AddComponent<GameManager>();

        GameObject spawnerObject = new GameObject("CoinSpawner");
        CoinSpawner coinSpawner = spawnerObject.AddComponent<CoinSpawner>();

        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
        playerMovement.speed = 6f;

        EnemyChase enemyChase = enemy.GetComponent<EnemyChase>();
        enemyChase.speed = 4.5f;
        enemyChase.player = player;
        enemyChase.gameManager = gameManager;

        coinSpawner.coinPrefab = coinPrefab;
        coinSpawner.coinsPerBatch = 5;
        coinSpawner.spawnMin = new Vector2(-7.6f, -3.8f);
        coinSpawner.spawnMax = new Vector2(7.6f, 3.8f);
        coinSpawner.coinValues = new int[] { 50, 20, 10 };
        coinSpawner.coinColors = new Color[] { ChipOrange, ChipGreen, ChipBlue };
        coinSpawner.gameManager = gameManager;

        gameManager.targetScore = 1000;
        gameManager.coinSpawner = coinSpawner;
        gameManager.player = player;
        gameManager.enemy = enemy;

        EditorUtility.SetDirty(playerMovement);
        EditorUtility.SetDirty(enemyChase);
        EditorUtility.SetDirty(coinSpawner);
        EditorUtility.SetDirty(gameManager);

        // h2. extensions (each one is its own component, see CLAUDE.md "Extensions")
        BuildExtensions(player, enemy, playerMovement, gameManager, coinSpawner, capsuleSprite, squareSprite,
            shadowSprite, out bool hudBuilt);

        // i. save, build settings, selection
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, ScenePath);
        MakeFirstInBuildSettings(ScenePath);

        Selection.activeGameObject = managerObject;
        EditorGUIUtility.PingObject(managerObject);

        Debug.Log("Assignment01Builder: scene built and saved to " + ScenePath + "\n" +
                  "- Player (-4, 0): Dynamic Rigidbody2D, capsule collider, PlayerMovement speed 6, PlayerDash\n" +
                  "- Enemy (5, 3): Kinematic Rigidbody2D, trigger capsule collider, EnemyChase speed 4.5, EnemyDifficulty\n" +
                  "- Coin prefab: " + CoinPrefabPath + " (trigger circle collider, no Rigidbody2D, CoinPopIn)\n" +
                  "- CoinSpawner: 5 coins per batch, values 50/20/10, range (-7.6, -3.8) to (7.6, 3.8)\n" +
                  "- GameManager: target score 1000\n" +
                  "- Knight (hidden until batch 3), chessboard background\n" +
                  "- HUD: " + (hudBuilt ? "built" : "SKIPPED (import TMP Essential Resources, then run the builder again)") + "\n" +
                  "- Background: " + (useCustomFelt ? FeltCustomPath : FeltPath) + "\n" +
                  "Press Play (Game view set to 16:9 so the whole table is visible).");
    }

    // ================================================================ extensions

    static void BuildExtensions(GameObject player, GameObject enemy, PlayerMovement playerMovement,
        GameManager gameManager, CoinSpawner coinSpawner, Sprite capsuleSprite, Sprite squareSprite,
        Sprite shadowSprite, out bool hudBuilt)
    {
        // Dash: values come from the script defaults (2.5x, 0.15 s, 1.5 s cooldown).
        PlayerDash dash = player.AddComponent<PlayerDash>();
        dash.speedMultiplier = 2.5f;
        dash.dashDuration = 0.15f;
        dash.cooldown = 1.5f;
        EditorUtility.SetDirty(dash);

        // Difficulty on the red enemy.
        EnemyDifficulty difficulty = enemy.AddComponent<EnemyDifficulty>();
        difficulty.speedIncreasePerBatch = 0.05f;
        difficulty.maxSpeedPercentOfPlayer = 0.9f;
        difficulty.coinSpawner = coinSpawner;
        difficulty.playerMovement = playerMovement;
        EditorUtility.SetDirty(difficulty);

        // Knight and its landing marker.
        KnightEnemy knight = CreateKnight(player, gameManager, coinSpawner, capsuleSprite, squareSprite, shadowSprite);

        // HUD (needs the TextMeshPro essentials).
        hudBuilt = CreateHud(gameManager, coinSpawner, knight);
    }

    static KnightEnemy CreateKnight(GameObject player, GameManager gameManager, CoinSpawner coinSpawner,
        Sprite capsuleSprite, Sprite squareSprite, Sprite shadowSprite)
    {
        // Marker: a square on the landing cell, hidden until the knight shows it.
        GameObject marker = CreateSpriteObject("KnightMarker", squareSprite, MarkerColor, OrderMarker,
            new Vector3(0f, 0f, 0f), new Vector3(0.9f, 0.9f, 1f));
        marker.SetActive(false);

        // Knight: dark capsule, a bit bigger than the pawns, Kinematic + trigger like the red enemy.
        GameObject knight = CreateSpriteObject("Knight", capsuleSprite, KnightColor, OrderKnight,
            new Vector3(7.5f, 3.5f, 0f), new Vector3(KnightScale, KnightScale, 1f));
        knight.tag = "Enemy";
        SpriteRenderer bodyRenderer = knight.GetComponent<SpriteRenderer>();

        Rigidbody2D body = knight.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D capsule = knight.AddComponent<CapsuleCollider2D>();
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(PawnWidth, PawnHeight);
        capsule.offset = Vector2.zero;
        capsule.isTrigger = true;

        // Thin light outline: the same capsule, slightly bigger, drawn just behind the body.
        GameObject rim = new GameObject("Rim");
        rim.transform.SetParent(knight.transform, false);
        rim.transform.localScale = new Vector3(1.12f, 1.06f, 1f);
        SpriteRenderer rimRenderer = rim.AddComponent<SpriteRenderer>();
        rimRenderer.sprite = capsuleSprite;
        rimRenderer.color = KnightRimColor;
        rimRenderer.sortingOrder = OrderKnight - 1;

        SpriteRenderer shadowRenderer = AddShadow(knight.transform, shadowSprite, PawnShadowOffset, PawnShadowScale);

        KnightEnemy script = knight.AddComponent<KnightEnemy>();
        script.gameManager = gameManager;
        script.coinSpawner = coinSpawner;
        script.player = player;
        script.marker = marker;
        script.visuals = new SpriteRenderer[] { bodyRenderer, rimRenderer, shadowRenderer };
        script.appearAtBatch = 3;
        script.moveInterval = 1.2f;
        script.markerTime = 0.5f;
        script.hopDuration = 0.15f;
        script.gridMin = new Vector2(-7.5f, -3.5f);
        script.gridMax = new Vector2(7.5f, 3.5f);
        EditorUtility.SetDirty(script);

        // Saved hidden: with the KnightEnemy script switched off this is the original game.
        for (int i = 0; i < script.visuals.Length; i++)
        {
            script.visuals[i].enabled = false;
        }
        capsule.enabled = false;

        return script;
    }

    static bool CreateHud(GameManager gameManager, CoinSpawner coinSpawner, KnightEnemy knight)
    {
        // Without the TMP essentials there is no default font and the texts would stay empty.
        if (!File.Exists(ToFullPath(TmpSettingsPath)))
        {
            Debug.LogWarning("Assignment01Builder: TextMeshPro Essential Resources are missing, so the HUD was skipped.\n" +
                             "Fix: Window > TextMeshPro > Import TMP Essential Resources, then run the builder again.");
            return false;
        }

        GameObject canvasObject = new GameObject("HUD");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        Transform parent = canvasObject.transform;
        Color32 gold = new Color32(0xFF, 0xD8, 0x4D, 255);

        TextMeshProUGUI score = CreateText(parent, "ScoreText", "0 / 1000", 56, White, TextAlignmentOptions.TopLeft,
            new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(40f, -30f), new Vector2(600f, 90f));
        TextMeshProUGUI batch = CreateText(parent, "BatchText", "Batch 1", 48, White, TextAlignmentOptions.TopRight,
            new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-40f, -30f), new Vector2(600f, 90f));
        TextMeshProUGUI warning = CreateText(parent, "KnightWarningText", "Knight incoming!", 72, gold,
            TextAlignmentOptions.Center, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -150f),
            new Vector2(1200f, 120f));
        TextMeshProUGUI end = CreateText(parent, "EndText", "PROMOTED!", 170, gold, TextAlignmentOptions.Center,
            new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1600f, 260f));
        TextMeshProUGUI restart = CreateText(parent, "RestartText", "Press R to restart", 60, White,
            TextAlignmentOptions.Center, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -120f),
            new Vector2(1200f, 100f));

        // Hidden until needed (the Hud script switches them on).
        warning.gameObject.SetActive(false);
        end.gameObject.SetActive(false);
        restart.gameObject.SetActive(false);

        Hud hud = canvasObject.AddComponent<Hud>();
        hud.gameManager = gameManager;
        hud.coinSpawner = coinSpawner;
        hud.knight = knight;
        hud.scoreText = score;
        hud.batchText = batch;
        hud.warningText = warning;
        hud.endText = end;
        hud.restartText = restart;
        hud.warningTime = 2f;
        EditorUtility.SetDirty(hud);

        RestartOnR restartOnR = canvasObject.AddComponent<RestartOnR>();
        restartOnR.gameManager = gameManager;
        EditorUtility.SetDirty(restartOnR);

        return true;
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string content, float fontSize, Color color,
        TextAlignmentOptions alignment, Vector2 anchor, Vector2 pivot, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform));
        textObject.transform.SetParent(parent, false);

        RectTransform rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = size;

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.fontStyle = FontStyles.Bold;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    // ================================================================ tags

    static void EnsureTag(string tag)
    {
        SerializedObject tagManager = new SerializedObject(
            AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tags = tagManager.FindProperty("tags");

        for (int i = 0; i < tags.arraySize; i++)
        {
            if (tags.GetArrayElementAtIndex(i).stringValue == tag)
            {
                return;
            }
        }

        tags.InsertArrayElementAtIndex(tags.arraySize);
        tags.GetArrayElementAtIndex(tags.arraySize - 1).stringValue = tag;
        tagManager.ApplyModifiedProperties();
        AssetDatabase.SaveAssets();
    }

    // ================================================================ folders and files

    static string ToFullPath(string assetPath)
    {
        return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
    }

    static void EnsureFolder(string assetFolder)
    {
        if (AssetDatabase.IsValidFolder(assetFolder))
        {
            return;
        }

        string parent = Path.GetDirectoryName(assetFolder).Replace('\\', '/');
        string name = Path.GetFileName(assetFolder);
        AssetDatabase.CreateFolder(parent, name);
    }

    static void SaveTexture(string assetPath, Texture2D texture)
    {
        File.WriteAllBytes(ToFullPath(assetPath), texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);
    }

    static float GetTextureWidth(string assetPath)
    {
        // Read the PNG header through a temporary texture so a custom background of any size still covers the view.
        Texture2D temp = new Texture2D(2, 2);
        temp.LoadImage(File.ReadAllBytes(ToFullPath(assetPath)));
        float width = temp.width;
        Object.DestroyImmediate(temp);
        return width;
    }

    static Sprite ImportSprite(string assetPath, float pixelsPerUnit)
    {
        AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport);

        TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
    }

    // ================================================================ texture generators
    // All textures are white or gray (except felt) so SpriteRenderer.color can tint them.

    static Texture2D MakeCapsule(int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        float radius = width / 2f;
        float centerX = width / 2f;
        float lowCenterY = radius;
        float highCenterY = height - radius;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float px = x + 0.5f;
                float py = y + 0.5f;

                // Distance from the pixel to the vertical line segment that forms the spine of the capsule.
                float closestY = Mathf.Clamp(py, lowCenterY, highCenterY);
                float dx = px - centerX;
                float dy = py - closestY;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                // One pixel of soft edge = anti-aliasing.
                float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                pixels[y * width + x] = new Color32(255, 255, 255, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        return MakeTexture(width, height, pixels);
    }

    static Texture2D MakeChip(int size)
    {
        Color32[] pixels = new Color32[size * size];
        float radius = size / 2f;
        float ringRadius = radius * 0.72f;
        float ringHalfWidth = 2.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = x + 0.5f - radius;
                float dy = y + 0.5f - radius;
                float distance = Mathf.Sqrt(dx * dx + dy * dy);

                float alpha = Mathf.Clamp01(radius - distance + 0.5f);

                // Slightly gray inner ring: the chip must still read as a plain circle once tinted.
                float ring = Mathf.Clamp01(1f - Mathf.Abs(distance - ringRadius) / ringHalfWidth);
                byte gray = (byte)Mathf.RoundToInt(255f * (1f - 0.14f * ring));

                pixels[y * size + x] = new Color32(gray, gray, gray, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        return MakeTexture(size, size, pixels);
    }

    static Texture2D MakeShadow(int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x + 0.5f - halfWidth) / halfWidth;
                float ny = (y + 0.5f - halfHeight) / halfHeight;
                float distance = Mathf.Sqrt(nx * nx + ny * ny);

                // 1 in the middle, 0 at the edge of the ellipse, smooth in between.
                float t = Mathf.Clamp01(1f - distance);
                float alpha = t * t * (3f - 2f * t);
                pixels[y * width + x] = new Color32(0, 0, 0, (byte)Mathf.RoundToInt(alpha * 255f));
            }
        }

        return MakeTexture(width, height, pixels);
    }

    static Texture2D MakeSquare(int size)
    {
        Color32[] pixels = new Color32[size * size];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = new Color32(255, 255, 255, 255);
        }

        return MakeTexture(size, size, pixels);
    }

    static Texture2D MakeChessboard()
    {
        int width = BoardCellsX * BoardPixelsPerCell;
        int height = BoardCellsY * BoardPixelsPerCell;
        Color32[] pixels = new Color32[width * height];

        // Very low contrast: alternating faint dark and faint light cells, so the chips stay the most visible thing.
        Color32 darkCell = new Color32(0, 0, 0, 26);        // black at about 10%
        Color32 lightCell = new Color32(255, 255, 255, 13); // white at about 5%

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cellX = x / BoardPixelsPerCell;
                int cellY = y / BoardPixelsPerCell;
                pixels[y * width + x] = ((cellX + cellY) % 2 == 0) ? darkCell : lightCell;
            }
        }

        return MakeTexture(width, height, pixels);
    }

    static Texture2D MakeFelt(int width, int height)
    {
        Color32[] pixels = new Color32[width * height];
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x + 0.5f - halfWidth) / halfWidth;
                float ny = (y + 0.5f - halfHeight) / halfHeight;

                // 0 in the middle, 1 in the corners. Squared so the middle stays flat and the edges darken.
                float edge = Mathf.Clamp01(Mathf.Sqrt(nx * nx + ny * ny) / 1.4142f);
                float vignette = 1f - 0.4f * edge * edge;

                // Tiny per-pixel noise so the felt does not look like a flat color.
                float noise = 1f + (Hash01(x, y) - 0.5f) * 0.08f;
                float shade = vignette * noise;

                pixels[y * width + x] = new Color32(
                    (byte)Mathf.Clamp(Mathf.RoundToInt(FeltColor.r * shade), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(FeltColor.g * shade), 0, 255),
                    (byte)Mathf.Clamp(Mathf.RoundToInt(FeltColor.b * shade), 0, 255),
                    255);
            }
        }

        return MakeTexture(width, height, pixels);
    }

    // Deterministic pseudo random number in [0, 1] from two integers (same felt on every run).
    static float Hash01(int x, int y)
    {
        unchecked
        {
            int h = x * 374761393 + y * 668265263;
            h = (h ^ (h >> 13)) * 1274126177;
            h ^= h >> 16;
            return (h & 0xFFFF) / 65535f;
        }
    }

    static Texture2D MakeTexture(int width, int height, Color32[] pixels)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    // ================================================================ scene

    static Scene CreateScene()
    {
        string templatePath = FindUrpSceneTemplate();

        if (templatePath != null)
        {
            // The URP 2D template scene already holds Main Camera + Global Light 2D.
            // Saving it under a new path leaves the template itself untouched.
            Scene templateScene = EditorSceneManager.OpenScene(templatePath, OpenSceneMode.Single);
            EditorSceneManager.SaveScene(templateScene, ScenePath);
            return templateScene;
        }

        // No template found: build the same two objects by hand (Global Light 2D with default settings).
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();

        GameObject lightObject = new GameObject("Global Light 2D");
        Light2D globalLight = lightObject.AddComponent<Light2D>();
        globalLight.lightType = Light2D.LightType.Global;

        EditorSceneManager.SaveScene(scene, ScenePath);
        return scene;
    }

    static string FindUrpSceneTemplate()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Settings"))
        {
            return null;
        }

        string[] guids = AssetDatabase.FindAssets("t:Scene", new string[] { "Assets/Settings" });
        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (path != ScenePath)
            {
                return path;
            }
        }

        return null;
    }

    static void SetupCamera()
    {
        Camera camera = Camera.main;
        if (camera == null)
        {
            camera = Object.FindFirstObjectByType<Camera>();
        }

        camera.orthographic = true;
        camera.orthographicSize = 5f;
        camera.transform.position = new Vector3(0f, 0f, -10f);
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = FeltColor;
    }

    static GameObject CreateSpriteObject(string name, Sprite sprite, Color color, int sortingOrder,
        Vector3 position, Vector3 scale)
    {
        GameObject gameObject = new GameObject(name);
        gameObject.transform.position = position;
        gameObject.transform.localScale = scale;

        SpriteRenderer spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.color = color;
        spriteRenderer.sortingOrder = sortingOrder;
        return gameObject;
    }

    static void CreateWalls(Sprite squareSprite)
    {
        GameObject walls = new GameObject("Walls");

        // The square sprite is exactly 1 x 1 units, so the scale is the wall size.
        float horizontalLength = 2f * WallCenterX + WallThickness; // top and bottom walls
        float verticalLength = 2f * WallCenterY + WallThickness;   // left and right walls

        CreateWall(walls.transform, squareSprite, "Wall_Top", new Vector3(0f, WallCenterY, 0f),
            new Vector3(horizontalLength, WallThickness, 1f));
        CreateWall(walls.transform, squareSprite, "Wall_Bottom", new Vector3(0f, -WallCenterY, 0f),
            new Vector3(horizontalLength, WallThickness, 1f));
        CreateWall(walls.transform, squareSprite, "Wall_Left", new Vector3(-WallCenterX, 0f, 0f),
            new Vector3(WallThickness, verticalLength, 1f));
        CreateWall(walls.transform, squareSprite, "Wall_Right", new Vector3(WallCenterX, 0f, 0f),
            new Vector3(WallThickness, verticalLength, 1f));
    }

    static void CreateWall(Transform parent, Sprite squareSprite, string name, Vector3 position, Vector3 scale)
    {
        GameObject wall = CreateSpriteObject(name, squareSprite, RimColor, OrderRim, position, scale);
        wall.transform.SetParent(parent, true);

        // Plain (non-trigger) collider and no Rigidbody2D: a static wall.
        BoxCollider2D box = wall.AddComponent<BoxCollider2D>();
        box.size = Vector2.one;
        box.isTrigger = false;
    }

    static SpriteRenderer AddShadow(Transform parent, Sprite shadowSprite, Vector3 localPosition, Vector3 localScale)
    {
        GameObject shadow = new GameObject("Shadow");
        shadow.transform.SetParent(parent, false);
        shadow.transform.localPosition = localPosition;
        shadow.transform.localScale = localScale;

        // Decoration only: sprite renderer, no collider.
        SpriteRenderer spriteRenderer = shadow.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = shadowSprite;
        spriteRenderer.color = ShadowColor;
        spriteRenderer.sortingOrder = OrderShadow;
        return spriteRenderer;
    }

    // Player (isEnemy = false) and Enemy (isEnemy = true). They differ only in tag, color, body type and script.
    static GameObject CreatePawn(string name, string tag, Vector3 position, Color color,
        Sprite capsuleSprite, Sprite shadowSprite, bool isEnemy)
    {
        GameObject pawn = CreateSpriteObject(name, capsuleSprite, color, OrderPawn, position, Vector3.one);
        pawn.tag = tag;

        Rigidbody2D body = pawn.AddComponent<Rigidbody2D>();
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        CapsuleCollider2D capsule = pawn.AddComponent<CapsuleCollider2D>();
        capsule.direction = CapsuleDirection2D.Vertical;
        capsule.size = new Vector2(PawnWidth, PawnHeight);
        capsule.offset = Vector2.zero;

        if (isEnemy)
        {
            // Kinematic + trigger: fires the trigger against the Dynamic player and never pushes it.
            body.bodyType = RigidbodyType2D.Kinematic;
            capsule.isTrigger = true;
            pawn.AddComponent<EnemyChase>();
        }
        else
        {
            // Dynamic (no gravity, no rotation): gets trigger events from coins and is blocked by the walls.
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 0f;
            body.freezeRotation = true;
            capsule.isTrigger = false;
            pawn.AddComponent<PlayerMovement>();
        }

        // The shadow is an ellipse at the pawn's feet, nudged to the right so the light seems to come from the left.
        AddShadow(pawn.transform, shadowSprite, PawnShadowOffset, PawnShadowScale);
        return pawn;
    }

    static GameObject CreateCoinPrefab(Sprite chipSprite, Sprite shadowSprite)
    {
        GameObject coin = CreateSpriteObject("Coin", chipSprite, White, OrderCoin, Vector3.zero, Vector3.one);
        coin.tag = "Coin";

        // Trigger collider and NO Rigidbody2D, so two overlapping coins never produce an event.
        CircleCollider2D circle = coin.AddComponent<CircleCollider2D>();
        circle.radius = ChipDiameter / 2f;
        circle.offset = Vector2.zero;
        circle.isTrigger = true;

        coin.AddComponent<Coin>();

        // Extension: the coin grows from nothing to full size when it appears.
        CoinPopIn popIn = coin.AddComponent<CoinPopIn>();
        popIn.duration = 0.2f;

        // Round blob slightly below and to the right of the chip.
        AddShadow(coin.transform, shadowSprite, CoinShadowOffset, CoinShadowScale);

        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(coin, CoinPrefabPath);
        Object.DestroyImmediate(coin);
        return prefab;
    }

    static void MakeFirstInBuildSettings(string scenePath)
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
        scenes.Add(new EditorBuildSettingsScene(scenePath, true));

        EditorBuildSettingsScene[] existing = EditorBuildSettings.scenes;
        for (int i = 0; i < existing.Length; i++)
        {
            if (existing[i].path != scenePath)
            {
                scenes.Add(existing[i]);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }
}
