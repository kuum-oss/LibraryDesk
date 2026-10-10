#!/usr/bin/env python3
"""Fail when LibraryDesk.Core line coverage is below the requested threshold."""

from __future__ import annotations

import pathlib
import sys
import xml.etree.ElementTree as ET


def find_core_report(root: pathlib.Path) -> tuple[pathlib.Path, ET.Element]:
    for path in root.rglob("coverage.cobertura.xml"):
        report = ET.parse(path).getroot()
        package_names = {
            package.attrib.get("name", "")
            for package in report.findall("./packages/package")
        }
        if "LibraryDesk.Core" in package_names:
            return path, report
    raise FileNotFoundError("coverage.cobertura.xml для LibraryDesk.Core не знайдено")


def main() -> int:
    if len(sys.argv) != 3:
        print("Використання: check_coverage.py <тека результатів> <поріг>")
        return 2

    results = pathlib.Path(sys.argv[1])
    threshold = float(sys.argv[2])
    path, report = find_core_report(results)
    line_rate = float(report.attrib["line-rate"]) * 100
    branch_rate = float(report.attrib["branch-rate"]) * 100
    print(f"Звіт: {path}")
    print(f"Рядкове покриття: {line_rate:.2f}%")
    print(f"Покриття гілок: {branch_rate:.2f}%")
    if line_rate < threshold:
        print(f"Поріг {threshold:.2f}% не виконано.")
        return 1

    print(f"Поріг {threshold:.2f}% виконано.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
