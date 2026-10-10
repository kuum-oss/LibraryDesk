#!/usr/bin/env bash
set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
results_dir="$repo_root/artifacts/sr03-coverage"

cd "$repo_root"
rm -rf "$results_dir"

dotnet restore LibraryDesk.sln
dotnet format LibraryDesk.sln --verify-no-changes --no-restore
dotnet build LibraryDesk.sln -c Release --no-restore --warnaserror
dotnet test tests/Legacy.Tests/LibraryDesk.Legacy.Tests.csproj -c Release --no-build --no-restore
dotnet test LibraryDesk.Tests/LibraryDesk.Tests.csproj -c Release --no-build --no-restore \
  --collect:"XPlat Code Coverage" \
  --settings docs/sr03/coverlet.runsettings \
  --results-directory "$results_dir"
python3 tools/check_coverage.py "$results_dir" 70
