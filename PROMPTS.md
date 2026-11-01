# פרומפטים למטלה 1 — לפי הסדר

הקובץ `CLAUDE.md` צריך לשבת בתיקיית השורש של פרויקט היוניטי (ליד `Assets`).
Claude Code טוען אותו אוטומטית. בתחילת הסשן הרץ `/context` וודא שהוא מופיע תחת Memory files.
את הפרומפטים מדביקים כמו שהם, באנגלית.

---

## פרומפט 1 — בנייה (מדביקים פעם אחת)

```
Read CLAUDE.md fully. Then implement Homework 1.

Deliverables:
1. The five runtime scripts in Assets/Scripts/Assignment01/, exactly as specified in CLAUDE.md
   ("Runtime architecture"), using only the "Allowed" list and nothing from the "Forbidden" list.

2. The local Editor builder Assets/_LocalTools/Editor/Assignment01Builder.cs with the menu item
   "Tools/Assignment 01/Build Scene". When run, it does, in order:
   a. Ensure the tags "Enemy" and "Coin" exist (TagManager.asset via SerializedObject). "Player" is built in.
   b. Generate sprites into Assets/Art/Generated/ only if they are missing. All anti-aliased and
      white/grayscale so SpriteRenderer.color can tint them:
      - Capsule.png 128x256 (rounded capsule)
      - Chip.png 128x128 (white disc with a subtle slightly-gray inner ring; it must still read as a plain circle)
      - Shadow.png 128x64 (soft black ellipse, alpha fading to 0 at the edge)
      - Square.png 4x4 (for the rim walls)
      - Felt.png 1024x576 (dark-green felt per the palette, subtle noise, vignette darker toward the edges)
      If Assets/Art/Felt_Custom.png exists, use it as the background instead of Felt.png.
      Import all as Sprite (Single), bilinear filtering, no compression, Pixels Per Unit chosen so the
      sizes in CLAUDE.md come out right (capsule and chip: 256 PPU).
   c. Create or overwrite Assets/Scenes/Assignment_01.unity. Start from the project's URP 2D scene
      template if one exists in Assets/Settings (it contains Main Camera + Global Light 2D). Otherwise create
      an empty scene with a Main Camera and one Global Light 2D with default settings, so lit sprites render
      normally — that is the template default, not added lighting.
   d. Camera: orthographic, size 5, position (0, 0, -10), solid background color = felt color.
   e. A Background object (felt sprite scaled to cover the full view) and the four rim walls
      (sizes, color and sorting from CLAUDE.md).
   f. Player at (-4, 0) and Enemy at (5, 3) with the components and settings from "Physics setup",
      correct tags, sprite colors, sorting orders, and a child "Shadow" sprite
      (local offset (0.06, -0.1), no collider).
   g. The Coin prefab at Assets/Prefabs/Coin.prefab (Chip sprite, CircleCollider2D trigger, Coin script,
      tag Coin, child "Shadow") saved with PrefabUtility.SaveAsPrefabAsset.
   h. GameManager and CoinSpawner objects. Wire every public reference and default value:
      coinValues 50 / 20 / 10, coinColors orange / green / blue from the palette,
      spawnMin (-7.6, -3.8), spawnMax (7.6, 3.8), targetScore 1000, player speed 6, enemy speed 4.5.
   i. Save the scene, make it the first scene in Build Settings, select GameManager in the Hierarchy,
      and Debug.Log a short summary of what was built.

3. .gitignore: the standard Unity ignores plus the extra entries listed in CLAUDE.md "Repo".

4. README.md draft in English per CLAUDE.md, with a placeholder for a screenshot/GIF.

Process: first reply with a short plan (file list + key decisions) and wait for my OK.
After writing the files, tell me: (1) the files you created, (2) exactly what I do in Unity, step by step,
(3) what I should see when I press Play.
```

---

## פרומפט 2 — בדיקת עמידה בדרישות (אחרי שהכול רץ)

```
Review mode — do not change any files yet.
1. For each runtime script, list every Unity API and C# feature it uses and mark each one
   Allowed / Forbidden according to CLAUDE.md.
2. Go through the 9 spec items in CLAUDE.md. For each one, name the file + method that implements it
   and any edge-case risk.
3. Check specifically: diagonal movement is normalized; speed is multiplied by fixedDeltaTime exactly once;
   "you won" / "you lose" are exact and printed once; the win can happen mid-batch; a new batch appears only
   after all 5 coins are collected; a coin can't be collected twice; enemy–coin overlap does nothing;
   there are no null-reference errors after the player or the enemy is destroyed.
4. Verify that .gitignore excludes Library, Temp, Obj, Logs and the local tools, and that nothing that will be
   committed references the builder.
Report as a table, then propose minimal fixes and wait for my OK.
```

---

## פרומפט 3 — שתבין כל שורה (מומלץ מאוד)

```
Explain each runtime script to me in simple Hebrew, line by line, as if preparing me for the lecturer
asking "why did you do it this way?". For each script, add 2 likely questions with short answers
(for example: why the player is Dynamic, why the spawner sets the coin's references,
why MovePosition runs in FixedUpdate).
```

---

## פרומפט 4 — תבנית לדיווח באג (בכל פעם שמשהו לא עובד)

```
Bug: <what I did> → <what I expected> → <what actually happened>.
Console (all red and yellow lines):
<paste here>
Find the root cause first and explain it in 2–3 sentences. Then propose the smallest fix within the
CLAUDE.md rules. If the fix belongs in the scene setup, change the builder and tell me to re-run it.
```

---

## פרומפט 5 — סגירת ה־README (אחרי שיש צילום מסך או GIF)

```
I added Docs/screenshot.png (or Docs/gameplay.gif). Finalize README.md: embed it, make sure the
requirement → implementation table matches the final code, and list the Inspector parameters with their
default values. Keep it short and clear.
```

---

## אופציונלי — רקע לבד מג'מיני או GPT

הרקע שהבונה מייצר מספיק. אם בכל זאת רוצים רקע עשיר יותר, שומרים את התמונה כ־`Assets/Art/Felt_Custom.png`
ומריצים את הבונה שוב.

```
Top-down view of a dark green casino-table felt surface. Seamless fabric texture with fine fibers,
very subtle variation, a soft vignette slightly darker at the edges, even soft lighting.
No objects, no chips, no cards, no text, no logos, no patterns. Matte, low contrast, calm.
16:9, 1920x1080.
```

ניגודיות נמוכה חשובה: הרקע לא אמור להתחרות בצ'יפים ובכלים.

---

## בדיקות לפני הגשה

1. WASD מזיז את השחקן, אלכסון לא מהיר יותר, ואי אפשר לצאת מהשולחן.
2. האויב רודף כל הזמן, ושינוי המהירות ב־Inspector עובד.
3. בהתחלה יש חמישה צ'יפים בתוך הטווח, בסוגים ובצבעים אקראיים.
4. שינוי ערכים, צבעים וטווח ב־Inspector משפיע על המשחק.
5. איסוף מעלים את הצ'יפ מיד ומדפיס לוג התקדמות.
6. האויב שעובר על צ'יפ — לא קורה כלום.
7. אחרי שכל החמישה נאספו מופיע גל חדש.
8. ב־1000: "you won" פעם אחת, והאויב נעלם.
9. מגע של האויב: "you lose" פעם אחת, בלי שגיאות אחר כך.
10. הקונסול נקי משגיאות ומאזהרות.
11. שכפול טרי של הריפו נפתח, והסצנה עובדת עם כל הרפרנסים.
