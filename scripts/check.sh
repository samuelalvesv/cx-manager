#!/usr/bin/env bash
set -euo pipefail

echo "==> 1. Verificando formatação do backend .NET (dotnet format)..."
dotnet format src/backend/CxManager/CxManager.slnx --verify-no-changes

echo "==> 2. Compilando solução do backend .NET 10..."
dotnet build src/backend/CxManager/CxManager.slnx

echo "==> 3. Executando testes do backend .NET 10..."
dotnet test --solution src/backend/CxManager/CxManager.slnx --no-build

echo "==> 4. Verificando linting do frontend (Oxlint)..."
npm --prefix src/frontend run lint

echo "==> 5. Verificando tipagem do frontend (TypeScript 7)..."
npm --prefix src/frontend run typecheck

echo "==> 6. Verificando compilação de produção do frontend (Vite)..."
npm --prefix src/frontend run build

echo "==> 7. Executando testes unitários do frontend (Vitest)..."
npm --prefix src/frontend run test

echo "==> ✅ Todas as verificações passaram com sucesso!"
