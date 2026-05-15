import re

with open(r"e:\New folder\EKLUS\SchoolErp\SchoolErp.Web\Components\Modules\Users\Users.razor", "r", encoding="utf-8") as f:
    content = f.read()

open_divs = len(re.findall(r"<div\b", content))
close_divs = len(re.findall(r"</div>", content))

print(f"Open divs: {open_divs}")
print(f"Close divs: {close_divs}")

# Find unbalanced blocks
lines = content.splitlines()
stack = []
for i, line in enumerate(lines):
    # Very simple tag matcher
    for match in re.finditer(r"<(div|/div)\b", line):
        tag = match.group(1)
        if tag == "div":
            stack.append(i + 1)
        else:
            if stack:
                stack.pop()
            else:
                print(f"Unexpected </div> at line {i + 1}")

if stack:
    for line_num in stack:
        print(f"Unclosed <div> at line {line_num}")
