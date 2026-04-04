# FamilyLedger

家庭共享记账项目，当前第一版重点是：

- 快速新增一笔支出
- 支持多人共享同一本账
- 为 iPhone 快捷指令截图识别预留后端接口

## 当前实现

- `Blazor Server + BootstrapBlazor + MongoDB.Driver`
- MongoDB 未配置时自动回退到内存仓储，便于本地演示
- 快速记账页：`/quick-entry`
- 账单流水页：`/transactions`
- 截图收件箱：`/capture-inbox`

## 预留接口

- `POST /api/family-ledger/quick-entry`
  - 用于移动端或小程序直接新增账单
- `POST /api/family-ledger/capture-drafts`
  - 用于 iPhone 快捷指令上传截图识别后的建议字段

## 下一步建议

- 接入真实 MongoDB 连接串
- 为截图收件箱补图像存储与 OCR/AI 识别
- 增加分类管理、成员管理、月度统计和导出
