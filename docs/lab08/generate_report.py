"""Generate the verified Lab 8 report for LibraryDesk."""

from pathlib import Path

from docx import Document
from docx.enum.section import WD_SECTION
from docx.enum.table import WD_CELL_VERTICAL_ALIGNMENT, WD_TABLE_ALIGNMENT
from docx.enum.text import WD_ALIGN_PARAGRAPH, WD_BREAK, WD_LINE_SPACING
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "output" / "docx" / "Звіт_ЛР8_LibraryDesk.docx"
EVIDENCE = ROOT / "docs" / "lab08" / "evidence"

BLACK = "000000"
NAVY = "17365D"
PALE_BLUE = "EAF2F8"
PALE_GRAY = "F3F4F6"
MID_GRAY = "D9D9D9"
TEXT_GRAY = "555555"


def set_repeat_table_header(row):
    tr_pr = row._tr.get_or_add_trPr()
    tbl_header = OxmlElement("w:tblHeader")
    tbl_header.set(qn("w:val"), "true")
    tr_pr.append(tbl_header)


def set_cell_shading(cell, fill):
    tc_pr = cell._tc.get_or_add_tcPr()
    shd = tc_pr.find(qn("w:shd"))
    if shd is None:
        shd = OxmlElement("w:shd")
        tc_pr.append(shd)
    shd.set(qn("w:fill"), fill)


def set_cell_margins(cell, top=90, start=110, bottom=90, end=110):
    tc = cell._tc
    tc_pr = tc.get_or_add_tcPr()
    tc_mar = tc_pr.first_child_found_in("w:tcMar")
    if tc_mar is None:
        tc_mar = OxmlElement("w:tcMar")
        tc_pr.append(tc_mar)
    for name, value in (("top", top), ("start", start), ("bottom", bottom), ("end", end)):
        node = tc_mar.find(qn(f"w:{name}"))
        if node is None:
            node = OxmlElement(f"w:{name}")
            tc_mar.append(node)
        node.set(qn("w:w"), str(value))
        node.set(qn("w:type"), "dxa")


def set_table_borders(table, color=MID_GRAY, size="6"):
    tbl_pr = table._tbl.tblPr
    borders = tbl_pr.find(qn("w:tblBorders"))
    if borders is None:
        borders = OxmlElement("w:tblBorders")
        tbl_pr.append(borders)
    for edge in ("top", "left", "bottom", "right", "insideH", "insideV"):
        tag = qn(f"w:{edge}")
        border = borders.find(tag)
        if border is None:
            border = OxmlElement(f"w:{edge}")
            borders.append(border)
        border.set(qn("w:val"), "single")
        border.set(qn("w:sz"), size)
        border.set(qn("w:space"), "0")
        border.set(qn("w:color"), color)


def set_font(run, name, size, bold=False, color=BLACK, italic=False):
    run.font.name = name
    run._element.get_or_add_rPr().rFonts.set(qn("w:ascii"), name)
    run._element.get_or_add_rPr().rFonts.set(qn("w:hAnsi"), name)
    run._element.get_or_add_rPr().rFonts.set(qn("w:eastAsia"), name)
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.italic = italic
    run.font.color.rgb = RGBColor.from_string(color)


def shade_paragraph(paragraph, fill=PALE_GRAY):
    p_pr = paragraph._p.get_or_add_pPr()
    shd = p_pr.find(qn("w:shd"))
    if shd is None:
        shd = OxmlElement("w:shd")
        p_pr.append(shd)
    shd.set(qn("w:fill"), fill)


def prevent_row_split(row):
    tr_pr = row._tr.get_or_add_trPr()
    cant_split = OxmlElement("w:cantSplit")
    tr_pr.append(cant_split)


def add_page_number(paragraph):
    paragraph.alignment = WD_ALIGN_PARAGRAPH.CENTER
    run = paragraph.add_run()
    begin = OxmlElement("w:fldChar")
    begin.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = " PAGE "
    separate = OxmlElement("w:fldChar")
    separate.set(qn("w:fldCharType"), "separate")
    end = OxmlElement("w:fldChar")
    end.set(qn("w:fldCharType"), "end")
    run._r.extend([begin, instr, separate, end])
    set_font(run, "Arial", 8, color=TEXT_GRAY)


def configure_document(doc):
    section = doc.sections[0]
    section.page_width = Inches(8.5)
    section.page_height = Inches(11)
    section.top_margin = Inches(0.62)
    section.bottom_margin = Inches(0.58)
    section.left_margin = Inches(0.68)
    section.right_margin = Inches(0.68)

    normal = doc.styles["Normal"]
    normal.font.name = "Times New Roman"
    normal._element.rPr.rFonts.set(qn("w:ascii"), "Times New Roman")
    normal._element.rPr.rFonts.set(qn("w:hAnsi"), "Times New Roman")
    normal.font.size = Pt(10.5)
    normal.font.color.rgb = RGBColor(0, 0, 0)
    normal.paragraph_format.space_after = Pt(4)
    normal.paragraph_format.line_spacing = 1.05

    title = doc.styles["Title"]
    title.font.name = "Arial"
    title._element.rPr.rFonts.set(qn("w:ascii"), "Arial")
    title._element.rPr.rFonts.set(qn("w:hAnsi"), "Arial")
    title.font.size = Pt(22)
    title.font.bold = True
    title.font.color.rgb = RGBColor(0, 0, 0)
    title.paragraph_format.space_after = Pt(18)

    for name, size in (("Heading 1", 14.5), ("Heading 2", 11.5)):
        style = doc.styles[name]
        style.font.name = "Arial"
        style._element.rPr.rFonts.set(qn("w:ascii"), "Arial")
        style._element.rPr.rFonts.set(qn("w:hAnsi"), "Arial")
        style.font.size = Pt(size)
        style.font.bold = True
        style.font.color.rgb = RGBColor(0, 0, 0)
        style.paragraph_format.space_before = Pt(5)
        style.paragraph_format.space_after = Pt(5)
        style.paragraph_format.keep_with_next = True

    caption = doc.styles["Caption"]
    caption.font.name = "Arial"
    caption._element.rPr.rFonts.set(qn("w:ascii"), "Arial")
    caption._element.rPr.rFonts.set(qn("w:hAnsi"), "Arial")
    caption.font.size = Pt(8)
    caption.font.bold = True
    caption.font.italic = False
    caption.font.color.rgb = RGBColor(0, 0, 0)
    caption.paragraph_format.space_before = Pt(3)
    caption.paragraph_format.space_after = Pt(5)
    caption.paragraph_format.keep_with_next = True

    for sec in doc.sections:
        footer = sec.footer
        add_page_number(footer.paragraphs[0])


def add_heading(doc, text, level=1):
    return doc.add_paragraph(text, style=f"Heading {level}")


def add_body(doc, text, bold_prefix=None):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.JUSTIFY
    p.paragraph_format.first_line_indent = Inches(0.28)
    if bold_prefix and text.startswith(bold_prefix):
        r1 = p.add_run(bold_prefix)
        set_font(r1, "Times New Roman", 10.5, bold=True)
        r2 = p.add_run(text[len(bold_prefix):])
        set_font(r2, "Times New Roman", 10.5)
    else:
        r = p.add_run(text)
        set_font(r, "Times New Roman", 10.5)
    return p


def add_bullet(doc, text, level=0):
    p = doc.add_paragraph(style="List Bullet")
    p.paragraph_format.left_indent = Inches(0.25 + 0.2 * level)
    p.paragraph_format.first_line_indent = Inches(-0.15)
    p.paragraph_format.space_after = Pt(2)
    r = p.add_run(text)
    set_font(r, "Times New Roman", 10)
    return p


def add_caption(doc, text):
    p = doc.add_paragraph(text, style="Caption")
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    return p


def add_listing(doc, title, text, font_size=8.2):
    add_caption(doc, title)
    p = doc.add_paragraph()
    p.paragraph_format.left_indent = Inches(0.16)
    p.paragraph_format.right_indent = Inches(0.16)
    p.paragraph_format.space_before = Pt(0)
    p.paragraph_format.space_after = Pt(6)
    p.paragraph_format.line_spacing_rule = WD_LINE_SPACING.SINGLE
    shade_paragraph(p, PALE_GRAY)
    r = p.add_run(text.rstrip())
    set_font(r, "Courier New", font_size)
    return p


def add_table(doc, headers, rows, widths, font_size=8.8):
    table = doc.add_table(rows=1, cols=len(headers))
    table.alignment = WD_TABLE_ALIGNMENT.CENTER
    table.autofit = False
    set_table_borders(table)
    header = table.rows[0]
    set_repeat_table_header(header)
    prevent_row_split(header)
    for idx, (cell, label) in enumerate(zip(header.cells, headers)):
        cell.width = Inches(widths[idx])
        cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
        set_cell_shading(cell, NAVY)
        set_cell_margins(cell, 95, 105, 95, 105)
        p = cell.paragraphs[0]
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(0)
        r = p.add_run(label)
        set_font(r, "Arial", font_size, bold=True, color="FFFFFF")
    for ridx, row_data in enumerate(rows):
        row = table.add_row()
        prevent_row_split(row)
        for idx, (cell, value) in enumerate(zip(row.cells, row_data)):
            cell.width = Inches(widths[idx])
            cell.vertical_alignment = WD_CELL_VERTICAL_ALIGNMENT.CENTER
            set_cell_margins(cell, 85, 105, 85, 105)
            if ridx % 2:
                set_cell_shading(cell, PALE_BLUE)
            p = cell.paragraphs[0]
            p.paragraph_format.space_after = Pt(0)
            p.alignment = WD_ALIGN_PARAGRAPH.CENTER if idx == 0 and len(headers) <= 4 else WD_ALIGN_PARAGRAPH.LEFT
            r = p.add_run(str(value))
            set_font(r, "Times New Roman", font_size)
    doc.add_paragraph().paragraph_format.space_after = Pt(0)
    return table


def add_image(doc, path, caption, width=6.65):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.keep_with_next = True
    p.paragraph_format.space_after = Pt(2)
    shape = p.add_run().add_picture(str(path), width=Inches(width))
    shape._inline.docPr.set("descr", caption)
    shape._inline.docPr.set("title", caption)
    add_caption(doc, caption)


def new_page(doc):
    doc.add_page_break()


def build_report():
    doc = Document()
    configure_document(doc)

    # Page 1: cover.
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_after = Pt(58)
    r = p.add_run("ЗВІТ")
    set_font(r, "Arial", 24, bold=True)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("до лабораторної роботи 8")
    set_font(r, "Arial", 16, bold=True)

    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_after = Pt(42)
    r = p.add_run("Складання статичний аналіз і безперервна інтеграція проєкту")
    set_font(r, "Arial", 14, bold=True)

    meta = [
        "Дисципліна: Конструювання програмного забезпечення",
        "Проєкт: LibraryDesk",
        "Варіант: 2",
        "Виконав: Гордєєв Дмитро Леонідович",
        "Група: ІПЗ",
        "Робоча гілка: feature/ci-pipeline",
    ]
    for line in meta:
        p = doc.add_paragraph()
        p.alignment = WD_ALIGN_PARAGRAPH.CENTER
        p.paragraph_format.space_after = Pt(7)
        r = p.add_run(line)
        set_font(r, "Times New Roman", 12)

    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(36)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("https://github.com/kuum-oss/LibraryDesk")
    set_font(r, "Courier New", 9.5)
    p = doc.add_paragraph()
    p.paragraph_format.space_before = Pt(44)
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    r = p.add_run("2026")
    set_font(r, "Times New Roman", 12)

    # Page 2: goal and source data.
    new_page(doc)
    add_heading(doc, "1 Мета роботи")
    add_body(
        doc,
        "Мета роботи полягає у приведенні навчального проєкту LibraryDesk до відтворюваного промислового циклу перевірки: упорядкувати історію Git, централізувати суворе складання, увімкнути статичний аналіз і форматування, створити GitHub Actions та довести, що quality gates зупиняють неякісний коміт.",
    )
    add_heading(doc, "2 Вихідні дані")
    add_body(
        doc,
        "Варіант 2 — LibraryDesk, система обліку видачі книг. Рішення працює на .NET 8 і поєднує доменне ядро, консольний застосунок, основний набір модульних тестів та окремий успадкований модуль із характеризаційними тестами.",
    )
    add_table(
        doc,
        ["Компонент", "Призначення"],
        [
            ("LibraryDesk.Core", "Доменні сутності, парсинг, політики, сервіси та звіти"),
            ("LibraryDesk.App", "Консольна точка входу та інфраструктурна композиція"),
            ("LibraryDesk.Tests", "54 модульні тести основного ядра"),
            ("src/Legacy", "Успадкований модуль попередніх лабораторних робіт"),
            ("tests/Legacy.Tests", "12 характеризаційних тестів успадкованого коду"),
        ],
        [1.8, 5.0],
    )
    add_heading(doc, "2 1 Підсумкова перевірка")
    add_table(
        doc,
        ["Перевірка", "Результат"],
        [
            ("Release-складання", "0 попереджень, 0 помилок"),
            ("Модульні тести", "66 із 66 пройдено"),
            ("Покриття рядків", "68,72 %, поріг 60 % пройдено"),
            ("Форматування", "dotnet format завершився з кодом 0"),
            ("Версія застосунку", "LibraryDesk v0.8.0"),
        ],
        [2.25, 4.55],
    )

    # Page 3: task 1.
    new_page(doc)
    add_heading(doc, "3 Завдання 1 Порядок у Git")
    add_body(
        doc,
        "Роботу виконано в гілці feature/ci-pipeline. Каталоги bin, obj, .vs та .idea не відстежуються; кожна логічна зміна оформлена окремим повідомленням Conventional Commits. Після завершення гілку синхронізовано з main.",
    )
    git_log = """bba1376 docs(report): додано звіт до лабораторної роботи 8
b868232 docs(ci): зафіксовано червоний і зелений запуски
f07e57e fix(ci): відновлено зелену збірку після перевірки
3c1ef0c test(ci): повторно зламано збірку для червоного запуску
b795a36 chore(review): виправлено зауваження самоогляду перед PR
a4f7626 feat(security): валідовано ввід і ввімкнено аудит NuGet
a405b9a fix(ci): усунено навмисне порушення складання
4538c0a test(ci): навмисне порушення для перевірки конвеєра"""
    add_listing(doc, "Лістинг 1  Вивід git log --oneline -n 8", git_log, 8.4)
    add_table(
        doc,
        ["Вимога", "Фактичний стан", "Оцінка"],
        [
            ("Гілка feature/", "feature/ci-pipeline", "виконано"),
            ("Не менше 5 комітів", "8 останніх комітів наведено вище", "виконано"),
            ("Тип і двокрапка", "build:, ci:, test(ci):, fix(ci):, docs:", "виконано"),
            ("Артефакти поза Git", "bin, obj, .vs, .idea не відстежуються", "виконано"),
        ],
        [1.75, 3.9, 1.15],
        8.6,
    )
    add_body(
        doc,
        "Дрібні коміти дозволили окремо побачити конфігурацію складання, workflow, навмисний дефект і його виправлення. Це спростило пошук причини червоного запуску та підтвердило відтворюваність історії.",
    )

    # Page 4: task 2.
    new_page(doc)
    add_heading(doc, "4 Завдання 2 Складання та версіювання")
    add_body(
        doc,
        "Файл Directory.Build.props централізує цільову платформу, nullable-контекст, C# 12, TreatWarningsAsErrors, аналізатори, стиль, версію 0.8.0, автора, продукт і NuGetAudit. Через це всі п'ять проєктів збираються за однаковими правилами.",
    )
    add_table(
        doc,
        ["Конфігурація", "Розмір Core.dll", "Час складання", "Оптимізація"],
        [
            ("Debug", "47 616 байтів", "3,75 с", "вимкнена"),
            ("Release", "45 568 байтів", "1,40 с", "увімкнена"),
        ],
        [1.55, 1.75, 1.55, 1.95],
        8.8,
    )
    add_body(
        doc,
        "Час залежить від стану кешу, тому його використано як характеристику контрольного запуску, а не як універсальний тест продуктивності. Менший Release-файл узгоджується з оптимізацією й вилученням частини налагоджувальної інформації.",
    )
    props = """<TargetFramework>net8.0</TargetFramework>
<Nullable>enable</Nullable>
<LangVersion>12.0</LangVersion>
<TreatWarningsAsErrors>true</TreatWarningsAsErrors>
<EnableNETAnalyzers>true</EnableNETAnalyzers>
<AnalysisLevel>latest-recommended</AnalysisLevel>
<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
<VersionPrefix>0.8.0</VersionPrefix>
<NuGetAudit>true</NuGetAudit>
<NuGetAuditMode>all</NuGetAuditMode>"""
    add_listing(doc, "Лістинг 2  Ключові властивості Directory.Build.props", props, 8.2)
    add_listing(doc, "Лістинг 3  Перший рядок запуску Release", "LibraryDesk v0.8.0", 9)
    add_body(
        doc,
        "VersionInfo читає AssemblyInformationalVersionAttribute і відкидає службові метадані після символу +. У результаті користувач бачить саме семантичну версію 0.8.0.",
    )

    # Page 5: task 3.
    new_page(doc)
    add_heading(doc, "5 Завдання 3 Форматування та статичний аналіз")
    add_body(
        doc,
        "StyleCop.Analyzers і вбудовані .NET Analyzers працюють під час складання. Навмисно дефектний ReportBuilder довів роботу правил, після чого виправлено також діагностики у власному коді Order.cs та успадкованому ReportBuilder.",
    )
    add_table(
        doc,
        ["Код", "Файл", "Суть", "Виправлення"],
        [
            ("CS0219", "Reports/ReportBuilder.cs", "змінну notUsed не використано", "змінну видалено"),
            ("CA1051, SA1401", "Reports/ReportBuilder.cs", "публічне змінюване поле Title", "властивість init"),
            ("CA1304, CA1311", "Reports/ReportBuilder.cs", "ToUpper залежить від культури", "ToUpperInvariant"),
            ("CA1305", "Order.cs", "ToString і Parse без культури", "InvariantCulture"),
            ("CA1305", "src/Legacy/ReportBuilder.cs", "формат грошей залежить від культури", "InvariantCulture"),
        ],
        [1.05, 2.0, 2.05, 1.7],
        7.8,
    )
    add_heading(doc, "5 1 Пояснення вимкнених правил", 2)
    disabled = [
        "SA0001 — службова діагностика XML-документації не додає корисного сигналу.",
        "SA1101 — обов'язковий this. суперечить прийнятій сучасній конвенції .NET.",
        "SA1200 — using поза простором імен відповідають file-scoped namespace.",
        "SA1204 — порядок статичних членів визначає логіка класу.",
        "SA1309 — приватні поля проєкту мають префікс підкреслення.",
        "SA1413 — кінцева кома в багаторядковому ініціалізаторі необов'язкова.",
        "SA1601, SA1623, SA1642 — шаблони XML не підходять до україномовної документації.",
        "SA1633 — ліцензійний заголовок не потрібний у навчальному репозиторії.",
    ]
    for item in disabled:
        add_bullet(doc, item)
    add_body(
        doc,
        "Контрольний результат: Release-складання — 0 попереджень і 0 помилок; dotnet format --verify-no-changes завершився з кодом 0.",
        "Контрольний результат:",
    )

    # Page 6: task 4 explanation and quality gates.
    new_page(doc)
    add_heading(doc, "6 Завдання 4 Конвеєр безперервної інтеграції")
    add_body(
        doc,
        "Workflow запускається для push у main і feature/**, а також для pull_request у main. Середовище ubuntu-latest отримує код, установлює .NET 8, відновлює залежності, перевіряє формат, збирає Release, виконує тести й контролює покриття.",
    )
    add_table(
        doc,
        ["Крок", "Команда або дія", "Що зупиняє конвеєр"],
        [
            ("Відновлення", "dotnet restore LibraryDesk.sln", "помилка залежності або аудиту NuGet"),
            ("Формат", "dotnet format --verify-no-changes", "файл потребує форматування"),
            ("Складання", "dotnet build -c Release", "будь-яке попередження або помилка"),
            ("Тести", "dotnet test -c Release", "хоча б один невдалий тест"),
            ("Покриття", "/p:Threshold=60", "покриття рядків нижче 60 %"),
            ("Артефакт", "actions/upload-artifact@v4", "звіт зберігається за if: always()"),
        ],
        [1.2, 2.8, 2.8],
        8.0,
    )
    add_heading(doc, "6 1 Обґрунтування порядку", 2)
    add_body(
        doc,
        "Форматування стоїть перед складанням, тому що це дешевша перевірка. Якщо змінено відступ або порядок using, конвеєр завершується до компіляції й тестів. Після одноразового restore наступні команди використовують --no-restore, а тести — також --no-build. Покриття рядків становить 68,72 %, тому поріг 60 % не знижувався.",
    )
    add_table(
        doc,
        ["Показник", "Факт", "Поріг"],
        [
            ("Попередження", "0", "0"),
            ("Тести", "66 / 66", "100 % зелених"),
            ("Покриття рядків", "68,72 %", "не менше 60 %"),
            ("Покриття гілок", "61,73 %", "інформаційно"),
            ("Покриття методів", "68,21 %", "інформаційно"),
        ],
        [2.15, 2.15, 2.5],
        8.6,
    )

    # Page 7: full workflow listing.
    new_page(doc)
    add_heading(doc, "6 2 Повний текст workflow")
    ci_text = (ROOT / ".github" / "workflows" / "ci.yml").read_text(encoding="utf-8")
    add_listing(doc, "Лістинг 4  Файл .github/workflows/ci.yml", ci_text, 6.8)
    add_body(
        doc,
        "Workflow містить сім змістових кроків. timeout-minutes обмежує завислий запуск, permissions: contents: read застосовує принцип мінімальних повноважень, а звіт cobertura зберігається як артефакт навіть після збою.",
    )

    # Page 8: red run.
    new_page(doc)
    add_heading(doc, "7 Завдання 5 Бейдж і зламана збірка")
    add_body(
        doc,
        "README містить робочий бейдж CI. Для перевірки quality gate у ReportBuilder було додано невикористану локальну змінну qualityGateProbe. Коміт 3c1ef0c запустив CI 2 і завершився помилкою через TreatWarningsAsErrors.",
    )
    add_image(doc, EVIDENCE / "ci-red.png", "Знімок екрана 1  Червоний запуск CI 2: Failure за 40 секунд", 6.7)
    error_text = """LibraryDesk.Core/Reports/ReportBuilder.cs(17,16): error CS0168:
The variable 'qualityGateProbe' is declared but never used.
Process completed with exit code 1."""
    add_listing(doc, "Лістинг 5  Текст помилки з журналу GitHub Actions", error_text, 8.4)
    add_body(
        doc,
        "Помилка виникла не під час виконання програми, а на етапі Release-складання. Це доводить, що сувора політика попереджень реально блокує зміну до проходження тестів.",
    )

    # Page 9: green run.
    new_page(doc)
    add_heading(doc, "7.1 Відновлення зеленої збірки")
    add_body(
        doc,
        "Окремий коміт f07e57e видалив навмисний дефект. CI 3 пройшов повний ланцюг quality gates: відновлення, форматування, Release-складання, 66 тестів і поріг покриття 60 %.",
    )
    add_image(doc, EVIDENCE / "ci-green.png", "Знімок екрана 2  Зелений запуск CI 3: Success за 45 секунд", 6.7)
    add_table(
        doc,
        ["Запуск", "Коміт", "Стан", "Тривалість", "Результат"],
        [
            ("CI 2", "3c1ef0c", "Failure", "40 с", "CS0168, exit code 1"),
            ("CI 3", "f07e57e", "Success", "45 с", "усі quality gates пройдено"),
        ],
        [0.75, 1.2, 1.05, 1.0, 2.8],
        8.0,
    )
    add_body(
        doc,
        "Зламана збірка має найвищий пріоритет, бо червоний main позбавляє команду надійної базової лінії. Кожен наступний коміт ускладнює відокремлення нового дефекту від вже наявного та збільшує вартість діагностики.",
    )

    # Page 10: task 6.
    new_page(doc)
    add_heading(doc, "8 Завдання 6 Чекліст безпечного коду")
    add_body(
        doc,
        "Зовнішній рядок ISBN;дні;денний тариф проходить через LoanItemParser.TryParse. Парсер перевіряє кількість полів, довжину ISBN, діапазон днів 1–365, тариф 0–100 000 та застосовує інваріантну культуру.",
    )
    add_table(
        doc,
        ["№", "Пункт", "Як перевірено", "Результат"],
        [
            ("1", "Ввід не потрапляє в Parse без перевірки", "LoanItemParser.TryParse", "небезпечний шлях замінено"),
            ("2", "Перевірено формат, межі та довжину ISBN", "9 невалідних рядків у Theory", "так"),
            ("3", "Секретів у коді немає", "git grep", "збігів немає"),
            ("4", "Секретів в історії немає", "git log -S ApiKey --all", "збігів немає"),
            ("5", "Локальні налаштування ігноруються", ".gitignore", ".env*, local settings, secrets JSON"),
            ("6", "Вразливих пакетів немає, аудит вбудовано", "dotnet list package, NuGetAudit", "0 пакетів; режим all"),
        ],
        [0.4, 2.55, 2.05, 1.8],
        7.6,
    )
    audit = """git grep -n -i -E "password|apikey|secret|token"
Результат: збігів у відстежуваному коді немає.

git log -p -S "ApiKey" --oneline --all
Результат: збігів в історії немає.

dotnet list LibraryDesk.sln package --vulnerable --include-transitive
Результат: відомих уразливих пакетів немає."""
    add_listing(doc, "Лістинг 6  Результати аудиту секретів і залежностей", audit, 7.9)
    add_body(
        doc,
        "Команда --outdated показує доступні оновлення, але не підтверджує вразливість. Команда --vulnerable порівнює пакети з базою відомих уразливостей; NuGetAudit повторює цю перевірку під час restore.",
    )

    # Page 11: task 7 and actual PR state.
    new_page(doc)
    add_heading(doc, "9 Завдання 7 Самоогляд і інтеграція")
    add_body(
        doc,
        "Перед інтеграцією проведено самоогляд різниці main...feature/ci-pipeline. Знайдені зауваження виправлено окремим комітом b795a36.",
    )
    add_table(
        doc,
        ["Зауваження", "Виправлення"],
        [
            ("README містив абсолютні шляхи локального Mac", "посилання замінено на відносні"),
            ("Перевірка parts.Length виконувалася запізно", "її перенесено перед доступом до поля"),
            ("Job не мав верхньої межі часу", "додано timeout-minutes: 10"),
        ],
        [3.35, 3.45],
        8.5,
    )
    add_image(doc, EVIDENCE / "pr-created.png", "Знімок екрана 3  Pull request 1 із заповненим самооглядом", 6.15)
    add_body(
        doc,
        "Pull request № 1 створено з гілки feature/ci-pipeline до main. Він містить один коміт bba1376, п'ять змінених файлів і 663 додані рядки. Опис охоплює виконані зміни та команди перевірки; усі сім пунктів самоогляду позначено як виконані. GitHub показує 2 із 2 успішних перевірок. На момент фіксації звіту PR відкритий і готовий до злиття.",
        "Pull request № 1 створено",
    )

    # Page 12: analysis and conclusions.
    new_page(doc)
    add_heading(doc, "10 Аналіз результату")
    add_body(
        doc,
        "Локальні та серверні перевірки узгоджені: однаковий порядок команд відтворює 0 попереджень, 66 зелених тестів і покриття рядків 68,72 %. Навмисна помилка CS0168 зупинила pipeline до тестів, а наступний окремий коміт повернув зелений стан. Отже, основні quality gates працюють не декларативно, а фактично.",
    )
    add_body(
        doc,
        "Найсильніша частина роботи — відтворюваний CI з порогом покриття та артефактом cobertura. Формальний етап рецензування також підтверджено через pull request № 1: опис і чекліст заповнено, а дві серверні перевірки завершилися успішно. Для повного завершення вимоги залишається злити відкритий PR у main.",
    )
    add_heading(doc, "11 Висновки")
    conclusions = [
        "1. Спільні параметри складання централізовано в Directory.Build.props, версію проєкту встановлено на 0.8.0.",
        "2. Release-складання проходить із 0 попереджень і 0 помилок; форматування не потребує змін.",
        "3. Конвеєр GitHub Actions контролює restore, формат, складання, 66 тестів і покриття рядків не менше 60 %.",
        "4. Червоний CI 2 довів роботу TreatWarningsAsErrors, а зелений CI 3 підтвердив виправлення окремим комітом.",
        "5. Зовнішній ввід перевіряється на межі модуля, секретів не знайдено, відомих уразливих пакетів немає.",
        "6. Pull request № 1 містить заповнений самоогляд і 2 із 2 зелених перевірок; на момент підготовки звіту він готовий до злиття.",
    ]
    for text in conclusions:
        p = doc.add_paragraph()
        p.paragraph_format.left_indent = Inches(0.15)
        p.paragraph_format.first_line_indent = Inches(-0.15)
        p.paragraph_format.space_after = Pt(4)
        r = p.add_run(text)
        set_font(r, "Times New Roman", 10.3)
    add_heading(doc, "11 1 Підсумкові посилання", 2)
    add_body(doc, "Репозиторій: https://github.com/kuum-oss/LibraryDesk")
    add_body(doc, "Червоний запуск: https://github.com/kuum-oss/LibraryDesk/actions/runs/38058131893")
    add_body(doc, "Зелений запуск: https://github.com/kuum-oss/LibraryDesk/actions/runs/38058340793")
    add_body(doc, "Pull request: https://github.com/kuum-oss/LibraryDesk/pull/1")
    add_body(doc, "Гілка: feature/ci-pipeline; фінальний коміт: bba1376.")

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    doc.core_properties.title = "Звіт до лабораторної роботи 8 LibraryDesk"
    doc.core_properties.subject = "Складання статичний аналіз і безперервна інтеграція"
    doc.core_properties.author = "Гордєєв Дмитро Леонідович"
    doc.core_properties.keywords = "LibraryDesk, CI, GitHub Actions, .NET 8, StyleCop"
    doc.save(OUTPUT)
    print(OUTPUT)


if __name__ == "__main__":
    build_report()
