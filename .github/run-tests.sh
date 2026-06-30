#!/usr/bin/env bash
# Runs `dotnet test` for every test project matching the given glob pattern.
#
# Usage: run-tests.sh '<glob>'   e.g.  run-tests.sh 'tests/*.UnitTests/*.csproj'
#
# Projects are discovered by the pattern, so adding a new test project that follows the
# naming convention needs no changes here or in the workflow. Every matched project runs
# even if an earlier one fails; the script exits non-zero if any project had failures.
set -uo pipefail

pattern="${1:?usage: run-tests.sh '<project-glob>'}"
results_dir="${GITHUB_WORKSPACE:-.}/test-results"

shopt -s nullglob globstar
projects=($pattern)

if [ ${#projects[@]} -eq 0 ]; then
  echo "No test projects matched '$pattern' — nothing to run."
  exit 0
fi

status=0
for proj in "${projects[@]}"; do
  name="$(basename "$proj" .csproj)"
  echo "::group::dotnet test $proj"
  dotnet test "$proj" \
    --configuration Release \
    --no-build \
    --logger "trx;LogFileName=${name}.trx" \
    --results-directory "$results_dir" || status=1
  echo "::endgroup::"
done

exit $status
