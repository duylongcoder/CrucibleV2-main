# Wrapper slash command (tùy chọn)

Skill gọi được trực tiếp bằng `/unity-game-studio <command> [tham số]`. Nếu muốn mỗi đầu việc có một lệnh riêng
(`/ugs-gdd`, `/ugs-build`, `/ugs-bug`...), copy các file trong `claude-commands/` vào:

- Theo dự án (chia sẻ cho team qua git): `<ProjectRoot>/.claude/commands/`
- Theo người dùng (mọi dự án): `~/.claude/commands/`

```bash
mkdir -p .claude/commands && cp <đường dẫn skill>/extras/claude-commands/ugs-*.md .claude/commands/
```

Mỗi wrapper chỉ chuyển tham số vào skill, không chứa logic riêng — sửa quy trình ở `commands/*.md` của skill.

| Lệnh | Vai trò | Việc |
|---|---|---|
| `/ugs-init` · `/ugs-status` · `/ugs-log` | Chung | Khởi tạo tài liệu · trạng thái · ghi log |
| `/ugs-concept` · `/ugs-systems` · `/ugs-gdd` · `/ugs-balance` · `/ugs-level` | Game Designer | Concept · hệ thống · GDD · cân bằng · màn chơi |
| `/ugs-art` · `/ugs-asset` · `/ugs-ui` | Designer | Art bible · asset · UI/UX |
| `/ugs-arch` · `/ugs-plan` · `/ugs-build` · `/ugs-scene` · `/ugs-refactor` · `/ugs-review` | Developer | Kiến trúc · plan · xây dựng · scene/prefab · refactor · review |
| `/ugs-test` · `/ugs-bug` · `/ugs-playtest` | QA / Tester | Test · bug · playtest |
| `/ugs-optimize` · `/ugs-setup` · `/ugs-ci` · `/ugs-release` | DevOps | Tối ưu · vệ sinh dự án · CI · phát hành |
