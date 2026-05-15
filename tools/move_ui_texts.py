import re
from pathlib import Path

# This script finds UI Text components where the text contains 'clone' or a known author
# and moves their RectTransform to top-right with anchored position -20,-20 and replaces
# the visible text with 'grupo CHOCHOCS'. It writes a .bak backup for each modified scene.

SCENES_DIR = Path('Assets/Scenes')
TARGET_ANCHOR = "{x: 1, y: 1}"
TARGET_POS = "{x: -20, y: -20}"
REPLACEMENT_TEXT = 'grupo CHOCHOCS'

scene_files = list(SCENES_DIR.glob('*.unity'))
if not scene_files:
    print('No scene files found in', SCENES_DIR)
    raise SystemExit(1)

for scene in scene_files:
    content = scene.read_text(encoding='utf-8')
    orig = content

    # Find MonoBehaviour blocks (UI Text) that have m_Text containing keywords
    mono_pattern = re.compile(r"--- !u!114 &(?P<mbid>\d+)[\s\S]*?m_GameObject: \{fileID: (?P<go>\d+)\}[\s\S]*?m_Text: (?P<text>.*?)(?=\n[^  ])", re.IGNORECASE)
    # collect game object ids that are credit texts
    credit_gos = set()
    for m in mono_pattern.finditer(content):
        text = m.group('text').strip().strip('"')
        if 'clone' in text.lower() or 'linhdvu' in text.lower() or 'clone by' in text.lower():
            credit_gos.add(m.group('go'))

    if not credit_gos:
        continue

    # Replace the m_Text value in those MonoBehaviour blocks
    def replace_text_block(match):
        go = match.group('go')
        if go in credit_gos:
            before = match.group(0)
            # replace m_Text: line
            new_block = re.sub(r"m_Text: .*", f"m_Text: {REPLACEMENT_TEXT}", before)
            return new_block
        return match.group(0)

    content = re.sub(r"(--- !u!114 &(?P<mbid>\d+)[\s\S]*?m_GameObject: \{fileID: (?P<go>\d+)\}[\s\S]*?)m_Text: .*?(?=\n[^ ])",
                     replace_text_block, content)

    # Now find RectTransform blocks that reference these game objects and change anchors/position
    rt_block_pattern = re.compile(r"(--- !u!224 &(?P<blockid>\d+)[\s\S]*?RectTransform:[\s\S]*?m_GameObject: \{fileID: (?P<go>\d+)\}[\s\S]*?)(?=--- !u!|$)")

    def replace_rt(match):
        block = match.group(1)
        go = match.group('go')
        if go not in credit_gos:
            return match.group(0)
        block = re.sub(r"m_AnchorMin: \{x: [^,]+, y: [^}]+\}", f"m_AnchorMin: {TARGET_ANCHOR}", block)
        block = re.sub(r"m_AnchorMax: \{x: [^,]+, y: [^}]+\}", f"m_AnchorMax: {TARGET_ANCHOR}", block)
        block = re.sub(r"m_AnchoredPosition: \{x: [^,]+, y: [^}]+\}", f"m_AnchoredPosition: {TARGET_POS}", block)
        return block

    content = rt_block_pattern.sub(replace_rt, content)

    if content != orig:
        backup = scene.with_suffix(scene.suffix + '.bak')
        backup.write_text(orig, encoding='utf-8')
        scene.write_text(content, encoding='utf-8')
        print(f'Updated scene: {scene} (backup: {backup})')

print('Done.')
