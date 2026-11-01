# 塔羅牌圖片提示詞

整副牌的圖分成兩種素材，**尺寸都是 7:12、滿版，像圖層一樣由下往上疊**：

1. **78 張插畫**（最下層）：每張牌一張滿版插畫，沒有邊框、文字、數字（「二、插畫」）。
2. **邊框範本**（上一層）：全副牌共用的一張華麗邊框，窗口去背透明（本檔「一、邊框範本」）。
3. **文字**（最上層，由程式畫）：羅馬數字、中英文牌名、逆位標示。不交給 AI 畫（AI 寫中文常亂碼，也無法保證 78 張字體一致）。

逆位只旋轉插畫層（以窗口中心為旋轉中心），邊框與文字保持正向。規格見 `00-master-plan.md` 的「牌面圖片」。

## 一、邊框範本（只生成一張）

整張牌 7:12 直式，建議 1400×2400 px（至少 840×1440；AI 做不到 7:12 時用最高直式比例生成再裁切）。窗口塗純洋紅 `#FF00FF` 方便去背；圓章與牌名板內部塗純黑，之後由程式寫字。

```text
A vertical ornate tarot card FRAME TEMPLATE, aspect ratio 7:12, full-bleed, flat 2D front view, no perspective, no shadow outside the card.

Style: dark mystical celestial tarot frame, deep midnight-navy and black background, delicate hand-drawn antique gold line work, double thin gold border lines with a small gap between them, rounded outer corners, intricate Art Nouveau / celestial filigree at all four corners (curling vines, tiny stars, crescent moons, small sparkles), fine gold dust speckles, subtle aged-gold metallic texture. Elegant, refined, thin lines, not thick or chunky. Perfectly symmetrical left-to-right.

Layout (percent of card height, measured from the top):
- Top center: an empty circular medallion about 11% of the card width in diameter, ringed in double gold lines, slightly overlapping the top border, with small crescent moons on its left and right. The inside of the medallion is SOLID BLACK and completely empty.
- Middle (about 5% to 83% of the height): a large rounded-rectangle window for the artwork, about 90% of the card width, thin gold inner border. The ENTIRE inside of this window is filled with a perfectly flat, solid pure magenta #FF00FF, with no gradient, no texture, no ornaments inside.
- Bottom (about 85% to 98%): a wide rounded name plate spanning the card width inside the border, edged with fine gold lines, small gold diamond and flourish ornaments at its left, right and bottom center, and a thin gold divider line in the middle. The inside of the plate is SOLID BLACK and completely empty.

Absolutely no text, letters, numbers, Roman numerals, symbols, watermark, characters, faces, or illustration anywhere. Only the frame, ornaments, the empty black medallion, the empty magenta window and the empty black name plate.

Output: highest resolution available, ideally 2800x4800 px.
```

生成後的處理：

- 先生成 3～4 張，挑一張最滿意的當範本，之後 78 張共用。
- 洋紅窗口若有漸層或雜點，請 AI 重畫，否則去背會留下邊緣。
- 量出洋紅窗口的外框（寬高與位置），記到 `assets/frame/frame.json`（`window: {x, y, width, height}`，單位為像素，以範本的實際尺寸為準）。程式用它決定逆位時的旋轉中心（窗口中心）。
- 存成 `assets/frame/frame.png`（洋紅已去背成透明的版本）。範本與插畫同為 7:12，疊圖時座標完全對齊。

## 二、插畫（78 張）

每張牌的完整提示詞 = 風格前綴 + 場景描述。比例 7:12 直式、**滿版**（背景鋪滿整張），建議 1400×2400 px（至少 840×1440）。檔名依 `image` 欄位，例如 `cards/major_00.webp`。

插畫會被邊框蓋住一部分（頂部圓章、下方牌名板、四周邊框與四角裝飾），所以**主體要避開這些位置**：主體留在左右各 10% 以內、上方 9% 到下方 80% 之間，視覺中心約在由上往下 44% 的位置；上方 8% 與下方 18% 只放背景的延伸（天空、地面、水面等），不放重要細節。這些寫在風格前綴裡。

### 風格前綴（每張都要貼在最前面）

```text
Semi-realistic anime-style watercolor illustration, dark midnight-navy starry atmosphere with warm glowing gold accents and cream-white highlights, delicate fine linework, soft painterly texture with gold dust sparkles, gentle emotional expressions, cinematic soft lighting, richly detailed, mystical and elegant mood. Vertical 7:12 portrait, full-bleed: the background fills the entire canvas edge to edge. Place the main subject so its visual center sits about 44% down from the top, entirely inside the central area (at least 10% clear of the left and right edges, below the top 9% and above the bottom 20% of the canvas). Keep the top 8% and the bottom 18% as plain atmospheric background (sky, ground, water, mist) with no important details, because a decorative frame will cover those areas. Illustration only: NO card border, NO frame, NO text, NO letters, NO numerals, NO captions, NO watermark.
```

## 各牌場景描述

### major_00　愚者 / The Fool

```text
Scene: A young woman with long flowing hair and a flower crown, eyes closed and face tilted up, stepping lightly toward the edge of a cliff with arms open. She wears a flowing cream dress with gold embroidery and carries a small leather satchel. A small white dog leaps playfully at her feet. Night sky with a large golden crescent moon, scattered stars, distant mountains and a winding river glowing in the valley below.
```

### major_01　魔術師 / The Magician

```text
Scene: A robed magician standing behind a wooden altar-table, one hand raised high holding a glowing wand toward the sky, the other pointing down toward the earth. On the table lie a golden cup, a sword, a coin engraved with a star and a second wand, one of each. A glowing sideways figure-eight of light floats above the head. Red roses and white lilies bloom around the table, dark star-filled sky behind.
```

### major_02　女祭司 / The High Priestess

```text
Scene: A serene woman in a flowing blue robe seated between two tall stone pillars, one black and one pale, with a veil of pomegranates hanging between them. A silver crescent moon rests at her feet, a tall lunar crown on her head, a partly hidden scroll in her lap. The hem of her robe turns into rippling water. Quiet, mysterious, moonlit atmosphere.
```

### major_03　皇后 / The Empress

```text
Scene: A gentle empress seated on a cushioned throne in a golden wheat field, wearing a crown of twelve small stars and a flowing gown patterned with pomegranates. A heart-shaped shield rests beside her and a short scepter in her hand. Lush forest and a small waterfall behind, warm glowing light, abundant and nurturing mood.
```

### major_04　皇帝 / The Emperor

```text
Scene: A stern emperor seated on a heavy stone throne carved with ram heads, wearing a red robe over steel armor, a golden crown, holding an ankh-topped scepter in one hand and a golden orb in the other. Behind him bare, rocky mountains and a thin winding river under a dark orange-tinted sky. Authority and structure.
```

### major_05　教皇 / The Hierophant

```text
Scene: A hierophant in rich ceremonial robes seated between two stone pillars, wearing a triple-tiered crown, one hand raised in blessing and the other holding a tall triple-crossed staff. Two crossed keys lie at his feet, two small kneeling figures in robes before him. Calm sacred hall lit by warm gold light.
```

### major_06　戀人 / The Lovers

```text
Scene: Two figures in flowing robes standing apart in a lush garden under a huge radiant angel with red wings floating in the clouds above them, hands outstretched in blessing. Behind the woman a tree with fruit and a coiled serpent; behind the man a tree with flame-like leaves. A tall mountain glows between them in the distance. Tender, romantic, modest.
```

### major_07　戰車 / The Chariot

```text
Scene: A crowned warrior in star-patterned armor standing in a chariot under a canopy of stars, holding a wand, with no reins. Two sphinxes, one black and one white, rest in front of the chariot. A walled city and a river behind. Determined and victorious mood.
```

### major_08　力量 / Strength

```text
Scene: A calm woman in a white gown with a garland of flowers, a glowing sideways figure-eight above her head, gently holding and closing the jaws of a golden lion that bows to her. Soft hills and a distant blue mountain behind. Quiet, gentle strength.
```

### major_09　隱者 / The Hermit

```text
Scene: An old hooded figure in a long gray cloak standing alone on a snowy mountain peak under a night sky, holding a lantern that glows with a six-pointed star, and a tall staff in the other hand. Deep, silent landscape with distant cold peaks. Solitary and wise.
```

### major_10　命運之輪 / Wheel of Fortune

```text
Scene: A great golden wheel floating in a swirling sky, ringed with abstract alchemical glyphs (no readable letters). On top a seated sphinx holding a sword, on the right a jackal-headed figure rising, on the left a serpent sinking. In the four corners of the cloud four winged beings, an angel, an eagle, a bull and a lion, each reading an open book.
```

### major_11　正義 / Justice

```text
Scene: A crowned figure in red robes seated on a throne between two stone pillars with a purple veil behind, holding an upright sword in the right hand and a pair of balanced scales in the left. Solemn, fair, quiet atmosphere with a soft glow around the scales.
```

### major_12　倒吊人 / The Hanged Man

```text
Scene: A figure hanging upside down by one foot from a living, leafy T-shaped tree, the other leg bent behind, hands tied behind the back, with a calm peaceful face and a golden halo around the head. Pale dawn sky behind. Suspended, serene, still.
```

### major_13　死神 / Death

```text
Scene: A skeleton knight in black armor riding a white horse and carrying a black banner with a white rose. Before him a fallen king, a praying bishop in tall hat, a kneeling child and a maiden. In the far background a river and the sun rising between two tall towers. Solemn but hopeful.
```

### major_14　節制 / Temperance

```text
Scene: A tall winged angel with a golden halo, one foot on land and one foot in a calm pond, pouring water between two cups. A winding path leads to distant mountains with a glowing golden crown of light above them. Irises bloom at the water's edge. Balanced, soft, flowing.
```

### major_15　惡魔 / The Devil

```text
Scene: A horned, bat-winged goat-headed figure squatting on a black stone pedestal, holding an upward torch, with an inverted fiery star above. Two small horned and tailed figures chained loosely at the neck at its feet, the chains are slack enough to slip off. Dark red-black cave, ominous but subtle.
```

### major_16　高塔 / The Tower

```text
Scene: A tall stone tower on a cliff struck by a bolt of lightning, the crown at the top blown off and flames bursting from the windows. Two figures falling headfirst through the dark sky, glowing sparks falling like rain. Dramatic, stormy, black sky.
```

### major_17　星星 / The Star

```text
Scene: A woman kneeling at the edge of a quiet pool, pouring water from two jugs, one onto the land and one into the pool, glowing sparkles. Above her one very large eight-pointed golden star surrounded by seven smaller stars, a bird in a tree nearby. Peaceful, hopeful, calm night.
```

### major_18　月亮 / The Moon

```text
Scene: A full moon with a serene face shining between two tall stone towers, drops of golden light falling. A dog and a wolf howling at the moon from either side of a winding path, a crayfish crawling out of a dark pool in the foreground. The path leads to distant mountains. Dreamy, uncertain.
```

### major_19　太陽 / The Sun

```text
Scene: A laughing child with curly hair and a laurel wreath, arms raised joyfully, riding a white horse with gold tack. A long red-orange ribbon streams in the wind. Behind them a huge radiant golden sun with wavy rays fills the upper half of the canvas. Rows of tall sunflowers in the foreground. Warm, joyful, glowing atmosphere, the one card with a lighter golden palette.
```

### major_20　審判 / Judgement

```text
Scene: A great angel with golden wings blowing a long trumpet from the clouds, a flag with a cross on the trumpet. Below, men, women and children with gray skin rising from stone coffins on the sea, arms raised toward the sound. Distant snow mountains. Solemn, awakening.
```

### major_21　世界 / The World

```text
Scene: A dancing figure wrapped in a flowing purple scarf, holding a short wand in each hand, surrounded by a large oval wreath of laurel leaves tied with ribbon. In the four corners an angel, an eagle, a bull and a lion among clouds. Celebratory, complete, glowing.
```

### wands_01　權杖王牌 / Ace of Wands

```text
Scene: A hand reaching out of a cloud holding a thick wooden staff that is budding with fresh green leaves, some leaves falling in the air. A distant castle on a hill and a calm river valley below. Rising energy.
```

### wands_02　權杖二 / Two of Wands

```text
Scene: A robed figure standing on a castle battlement at night holding a small globe in one hand and a budding wooden staff in the other, with a second staff fixed to the wall beside him. He looks out over the sea and distant mountains. Thoughtful, planning mood.
```

### wands_03　權杖三 / Three of Wands

```text
Scene: A figure seen from behind, standing on a cliff with a staff in the right hand and three staffs planted in the ground, looking out at ships sailing across a golden sea at sunrise. Wide open horizon.
```

### wands_04　權杖四 / Four of Wands

```text
Scene: Four tall wooden staffs forming a canopy garlanded with flowers and fruit, two figures under it raising bouquets in celebration, a castle with a bridge in the background. Joyful, warm, festive.
```

### wands_05　權杖五 / Five of Wands

```text
Scene: Five young people in mock combat swinging wooden staffs at each other in a field, nobody winning, dust and motion, dynamic and chaotic composition. Clear sky with stern light.
```

### wands_06　權杖六 / Six of Wands

```text
Scene: A rider in a laurel crown on a white horse draped in green cloth, holding a staff decorated with a laurel wreath. A crowd around him cheering and holding staffs. Victorious, bright atmosphere.
```

### wands_07　權杖七 / Seven of Wands

```text
Scene: A figure standing on a hilltop in mismatched boots, holding a staff in a defending stance, looking down at six wooden staffs rising from below to attack. Determined, standing alone.
```

### wands_08　權杖八 / Eight of Wands

```text
Scene: Eight budding wooden staffs flying diagonally through a clear sky above a green river valley and a distant house, parallel and fast, no people. Speed and arrival.
```

### wands_09　權杖九 / Nine of Wands

```text
Scene: A wounded figure with a bandaged head leaning on a staff, watching warily over his shoulder, with eight staffs standing in a row like a fence behind him. Tired but unyielding.
```

### wands_10　權杖十 / Ten of Wands

```text
Scene: A bent figure carrying a heavy bundle of ten wooden staffs on the back, walking toward a small village in the distance at dusk. Heavy burden, close to home.
```

### wands_11　權杖侍者 / Page of Wands

```text
Scene: A young page in a patterned tunic and feathered hat standing in a desert landscape, holding a budding staff and looking at its tip with curiosity. Dunes and a small pyramid-like hill in the distance.
```

### wands_12　權杖騎士 / Knight of Wands

```text
Scene: A knight in armor and a surcoat patterned with small salamanders, riding a rearing horse across a desert, holding a budding staff. Pyramids far in the background. Energetic, eager to go.
```

### wands_13　權杖皇后 / Queen of Wands

```text
Scene: A confident queen on a throne carved with lions, holding a sunflower in one hand and a budding staff in the other, a black cat at her feet. Orange-gold gown, desert backdrop with distant peaks.
```

### wands_14　權杖國王 / King of Wands

```text
Scene: A king on a throne decorated with lions and salamanders, wearing a crown and an orange cloak, holding a budding staff. Warm gold lighting, dry mountains behind. Strong, visionary.
```

### cups_01　聖杯王牌 / Ace of Cups

```text
Scene: A hand reaching out of a cloud holding a golden cup that overflows with five streams of water falling into a pond covered in lotus flowers. A white dove descending with a small wafer into the cup. Calm and sacred.
```

### cups_02　聖杯二 / Two of Cups

```text
Scene: A man and a woman facing each other, each holding a golden cup raised toward the other, wreaths on their heads. Between them a winged lion-head staff, and a house on a hill in the far distance. Mutual, balanced.
```

### cups_03　聖杯三 / Three of Cups

```text
Scene: Three women in flowing gowns and flower garlands dancing in a circle, raising golden cups in a toast. Fruits and pumpkins at their feet in a harvest garden. Joyful celebration.
```

### cups_04　聖杯四 / Four of Cups

```text
Scene: A young man sitting under a tree with arms crossed, looking down at three golden cups in front of him. A hand from a cloud offers a fourth cup, which he doesn't notice. Quiet green landscape, bored and withdrawn.
```

### cups_05　聖杯五 / Five of Cups

```text
Scene: A cloaked figure in black, seen from behind, looking at three spilled cups on the ground, while two upright cups stand behind him, unnoticed. A river flows beside, a bridge and a distant castle on the other side. Gloomy sky.
```

### cups_06　聖杯六 / Six of Cups

```text
Scene: Two children in an old village courtyard, one offering a cup full of white flowers to the other. Six golden cups filled with flowers around them, soft sunlit stone houses. Nostalgic, innocent.
```

### cups_07　聖杯七 / Seven of Cups

```text
Scene: A silhouette of a person facing seven golden cups floating on clouds, each cup holding a different vision: a jewel, a castle, a laurel wreath, a dragon, a veiled figure, a snake and a glowing mist. Dreamlike, glittering.
```

### cups_08　聖杯八 / Eight of Cups

```text
Scene: A cloaked figure with a staff walking away from eight stacked golden cups, heading up a rocky path toward dark mountains. A large moon with a face in the night sky. Quiet departure.
```

### cups_09　聖杯九 / Nine of Cups

```text
Scene: A satisfied, well-dressed man seated on a bench with arms crossed, nine golden cups on a curved shelf behind him, a blue curtain behind. Contentment, comfort, warm lamplight.
```

### cups_10　聖杯十 / Ten of Cups

```text
Scene: A couple with arms raised in joy under a rainbow of ten golden cups arching across the sky, two children dancing beside them, and a cottage by a river and trees behind. Peaceful family happiness.
```

### cups_11　聖杯侍者 / Page of Cups

```text
Scene: A young page in a flower-patterned tunic standing by the sea, holding a golden cup from which a small fish peeks out. The page looks at the fish with surprise. Gentle waves, soft light.
```

### cups_12　聖杯騎士 / Knight of Cups

```text
Scene: A knight in armor with a winged helmet and a fish-patterned cloak riding a white horse at a slow walk, holding a golden cup forward. A calm river and rolling hills. Gentle, romantic.
```

### cups_13　聖杯皇后 / Queen of Cups

```text
Scene: A queen on an ornate throne shaped like a seashell at the water's edge, holding a richly decorated closed golden chalice with handles. Her gown ripples like water, pebbles at her feet. Dreamy, intuitive.
```

### cups_14　聖杯國王 / King of Cups

```text
Scene: A king on a throne floating on the sea, wearing a fish-shaped pendant, holding a golden cup in one hand and a short scepter in the other. A ship passing in the distance and a fish leaping nearby. Calm, composed.
```

### swords_01　寶劍王牌 / Ace of Swords

```text
Scene: A hand reaching out of a cloud gripping a sword pointing upward, with a golden crown on the tip, wrapped with an olive branch and a palm branch. Craggy mountain peaks below, clear cold light.
```

### swords_02　寶劍二 / Two of Swords

```text
Scene: A blindfolded woman in white, seated on a stone bench by a moonlit sea, holding two swords crossed over her chest. Jagged rocks in the water, a thin crescent moon above. Stalemate and silence.
```

### swords_03　寶劍三 / Three of Swords

```text
Scene: A large heart floating in a stormy sky, pierced by three swords, heavy rain falling around it. Dark gray clouds with soft gold edges. Grief, no people.
```

### swords_04　寶劍四 / Four of Swords

```text
Scene: A stone effigy of a knight lying on a tomb inside a quiet chapel, hands pressed together in prayer. Three swords hang on the wall behind and one sword lies under the tomb. Stained-glass window casting soft color. Rest and stillness.
```

### swords_05　寶劍五 / Five of Swords

```text
Scene: A smirking man in the foreground collecting three swords, two other swords on the ground, while two figures walk away dejected along the shore behind him. Stormy, ragged clouds.
```

### swords_06　寶劍六 / Six of Swords

```text
Scene: A ferryman poling a small boat across a river carrying a hooded woman and a child, six swords standing upright at the front of the boat. The water is rough on one side and calm on the other. Quiet passage toward a distant shore.
```

### swords_07　寶劍七 / Seven of Swords

```text
Scene: A figure with a wry look tiptoeing away from a military camp at dawn, carrying five swords in his arms, while two swords stay stuck in the ground behind him. Tents and a campfire in the distance.
```

### swords_08　寶劍八 / Eight of Swords

```text
Scene: A woman blindfolded and loosely bound in a cloth, standing among eight swords planted around her in wet ground. A castle on a cliff in the background, gray sky. The bindings are loose.
```

### swords_09　寶劍九 / Nine of Swords

```text
Scene: A person sitting up in bed at night with face in hands, nine swords hanging in a row on the dark wall behind. A quilt patterned with roses and zodiac symbols. Quiet anxiety.
```

### swords_10　寶劍十 / Ten of Swords

```text
Scene: A figure lying face down on dark ground with ten swords standing in the back, a black sky above and a faint golden sunrise on the horizon over calm water. Still and final, but hopeful at the edge.
```

### swords_11　寶劍侍者 / Page of Swords

```text
Scene: A young page standing on a windy hilltop, holding an upright sword with both hands, hair and clothes blowing in the wind. Clouds rushing and birds flying high. Alert, curious.
```

### swords_12　寶劍騎士 / Knight of Swords

```text
Scene: A knight in armor on a galloping white horse, charging forward with a raised sword. Windswept trees bend in the storm, clouds rush by, birds on the horse's cloth. Fast and fierce.
```

### swords_13　寶劍皇后 / Queen of Swords

```text
Scene: A dignified queen seated in profile on a stone throne carved with butterflies, in a dark blue cloak with silver embroidery, wearing a silver crown. She holds a tall upright sword in one hand and raises the other hand in a calm, stern gesture. Cool silver-blue sky with streaming clouds, a small bird flying in the distance. Composed, clear-minded, slightly melancholic mood, cool silver-blue accents against gold.
```

### swords_14　寶劍國王 / King of Swords

```text
Scene: A king seated on a stone throne carved with butterflies and crescent moons, wearing a blue robe and a crown, holding an upright sword. Tall clouds behind, cool silver-blue tones against gold. Clear, authoritative.
```

### pentacles_01　錢幣王牌 / Ace of Pentacles

```text
Scene: A hand reaching out of a cloud holding a large golden coin engraved with a five-pointed star. Below, a lush garden with a path through an arch of white lilies, leading to distant mountains. Quiet, fertile.
```

### pentacles_02　錢幣二 / Two of Pentacles

```text
Scene: A figure in a hat juggling two golden coins linked by an infinity-shaped ribbon, standing on a hill. Behind, two ships rising and falling on rolling sea waves. Lively balance.
```

### pentacles_03　錢幣三 / Three of Pentacles

```text
Scene: A craftsman standing on a bench carving inside a cathedral, while a monk and a nobleman hold architectural plans beside him. Three golden coins are carved into the stone arch above. Cooperation and craft.
```

### pentacles_04　錢幣四 / Four of Pentacles

```text
Scene: A crowned figure seated in front of a city, holding one golden coin tightly to the chest, one coin on the head and one coin under each foot. Protective, guarded.
```

### pentacles_05　錢幣五 / Five of Pentacles

```text
Scene: Two ragged figures, one on crutches, walking through falling snow past a lit stained-glass church window, in which five golden coins form a pattern. Cold night, warm light unreachable.
```

### pentacles_06　錢幣六 / Six of Pentacles

```text
Scene: A well-dressed merchant holding a balanced scale in one hand and giving coins with the other to two kneeling beggars. Six golden coins in the air around them. Dignified giving.
```

### pentacles_07　錢幣七 / Seven of Pentacles

```text
Scene: A gardener leaning on a hoe, looking at a leafy bush on which seven golden coins grow like fruit. Pause and evaluation, a quiet garden at dusk.
```

### pentacles_08　錢幣八 / Eight of Pentacles

```text
Scene: A craftsman sitting at a workbench, carefully engraving a star into a golden coin, six finished coins displayed on a post and the floor, and a small town in the distance. Focused, patient work.
```

### pentacles_09　錢幣九 / Nine of Pentacles

```text
Scene: A woman in a rich gold-embroidered gown standing in a vineyard, a falcon on her gloved hand, nine golden coins among the grapevines, a snail on the ground. Elegant, self-sufficient.
```

### pentacles_10　錢幣十 / Ten of Pentacles

```text
Scene: An old man seated under an archway with two dogs, a couple and a child in the middle, ten golden coins in a pattern in the air and on the arch, a castle behind. Legacy and family.
```

### pentacles_11　錢幣侍者 / Page of Pentacles

```text
Scene: A young page standing in a green field holding up a golden coin and looking at it intently. Plowed fields and trees behind, soft morning light. Curious, attentive.
```

### pentacles_12　錢幣騎士 / Knight of Pentacles

```text
Scene: A knight in armor sitting still on a stout dark horse in a plowed field, holding a golden coin in front of him. Steady, patient, reliable.
```

### pentacles_13　錢幣皇后 / Queen of Pentacles

```text
Scene: A queen on a throne carved with fruit and flowers, in a garden of roses and vines, holding a golden coin in her lap, a small rabbit at her feet. Warm, nurturing.
```

### pentacles_14　錢幣國王 / King of Pentacles

```text
Scene: A king seated on a throne decorated with bulls and grapevines, wearing a robe embroidered with grapes, holding a scepter and a golden coin on his knee. A castle in the background. Prosperous, steady.
```
