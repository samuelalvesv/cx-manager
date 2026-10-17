---
name: rtk-rules
description: Token-optimized CLI proxy rules for executing shell commands via rtk.
trigger: always_on
---

# RTK - Rust Token Killer

**Usage**: Token-optimized CLI proxy for shell commands.

## Rule

Always prefix shell commands with `rtk` to minimize token consumption.

Examples:

```bash
rtk git status
rtk dotnet build src/backend/CxManager/CxManager.slnx
rtk dotnet test --solution src/backend/CxManager/CxManager.slnx
rtk npm run lint
rtk ls src/
rtk grep "pattern" src/
rtk find "*.cs" .
```

## Meta Commands

```bash
rtk gain              # Show token savings
rtk gain --history    # Command history with savings
rtk discover          # Find missed RTK opportunities
rtk proxy <cmd>       # Run raw (no filtering, for debugging)
```

## Why

RTK filters and compresses command output before it reaches the LLM context, cutting up to 90% of the bash output on common operations. Always use `rtk <cmd>` instead of raw commands.

## Permissões de Execução Segura

A execução de comandos de terminal deve utilizar obrigatoriamente o prefixo `rtk`:
- Apenas subcomandos explícitos de leitura, inspeção, lint, compilação e execução de testes são permitidos automaticamente (ex: `rtk git status`, `rtk git log`, `rtk dotnet test`, `rtk npm run build`).
- Comandos com efeitos colaterais, mutações de controle de versão ou exclusão destrutiva (`git commit`, `git add`, `git push`, `rm -rf`) são estritamente proibidos.
