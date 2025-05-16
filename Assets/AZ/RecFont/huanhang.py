

with open('2350-common-hangul.txt', 'r', encoding='utf-8') as f:
    content = f.read()
clearn = content.replace('\n', '').replace('\r', '')

with open('newfile.txt', 'w', encoding='utf-8') as f:
    f.write(clearn)
