#!/usr/bin/env bash
set -euo pipefail

echo "==> 1. Executando testes do backend .NET 10..."
dotnet test --solution src/backend/CxManager/CxManager.slnx

echo "==> 2. Verificando linting do frontend (Oxlint)..."
npm --prefix src/frontend run lint

echo "==> 3. Verificando tipagem do frontend (TypeScript 7)..."
npm --prefix src/frontend run typecheck

echo "==> 4. Executando testes unitários do frontend (Vitest)..."
npm --prefix src/frontend run test

echo "==> 5. Verificando compilação de produção do frontend (Vite)..."
npm --prefix src/frontend run build

echo "==> ✅ Todas as verificações passaram com sucesso!"
