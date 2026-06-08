# EduTrack

Bộ core khởi đầu cho dự án EduTrack, clone từ TDSolution (OrderDebt) chỉ giữ lại các phần dùng chung.

## Cấu trúc

```
EduTrack/
├── EduTrack.sln
├── TD.Lib/                    # Generic UoW + Repository + AutoMapper + Response + JwtHelper
├── EduTrack.API/              # ASP.NET Core 8 Web API
│   ├── Attributes/            # TDModule, TDPermission, TDAuthorize
│   ├── Commons/               # Constants (RoleCodes), ErrorMessage, ValidateData, SqlLogQuery
│   ├── Configs/               # ServiceRegisters (DI + JWT)
│   ├── Controllers/
│   │   ├── Base/              # ApiController + BaseController<T,TDto>
│   │   ├── Common/            # CommonController (dropdowns), FileController, ExcelController
│   │   ├── AuthController.cs
│   │   ├── UserController.cs
│   │   ├── RoleController.cs
│   │   ├── PageController.cs
│   │   └── ConfigJsonController.cs
│   ├── DataContext/
│   │   ├── EduTrackDbContext.cs  # Soft delete + audit + MarkDirty
│   │   ├── DapperContext.cs
│   │   ├── Entity/Core/       # User, Role, Permission, Page, File, UserRole, RolePermission
│   │   ├── Entity/ConfigJson.cs
│   │   ├── Dto/Base/, Dto/Core/, AuthDto, ConfigJsonDto
│   │   └── Enums/Enums.cs     # GenderEnums
│   ├── Migrations/InitCore   # 1 migration cho 8 entity Core
│   ├── Services/Base/, Common/
│   ├── Services/Auth, User, Role, Page, ConfigJson
│   ├── UnitOfWork/
│   └── Program.cs
└── EduTrack.WEB/             # Angular 17 standalone + ng-zorro + ag-grid
    └── src/
        ├── app/
        │   ├── pages/
        │   │   ├── dashboard/
        │   │   └── system/    # auth (login + user), role, page, config-json
        │   ├── services/system/   # login, user, role, page, config-json, excel
        │   ├── shared/        # components, directives, pipes, modules, utils, interfaces, services
        │   ├── app.routes.ts
        │   └── app.config.ts
        ├── environment*.ts
        └── index.html
```

## Chạy local

### Backend
```bash
cd EduTrack.API
# Cập nhật ConnectionStrings:EduTrackDbContextConnection trong appsettings.json
dotnet ef database update
dotnet run
# Swagger: https://localhost:7246/swagger
```

### Frontend
```bash
cd EduTrack.WEB
npm install --legacy-peer-deps
npm start
# http://localhost:4200
```

## Bước tiếp theo

1. Cập nhật connection string SQL Server trong `EduTrack.API/appsettings.json`.
2. Cập nhật `Jwt:Key` thành chuỗi random đủ dài.
3. Chạy migration để tạo schema: `dotnet ef database update`.
4. Seed dữ liệu khởi tạo: tạo Role `SUPPER_ADMIN`/`ADMIN`, tạo user đầu tiên.
5. Thêm entity nghiệp vụ của EduTrack vào `DataContext/Entity/`, `Dto/`, service, controller theo pattern `BaseService<T,TDto>` / `BaseController<T,TDto>`.
6. Gọi `POST /api/role/scan-permission` để quét lại quyền từ controller attributes.

## Khác biệt với OrderDebt

- Đã xoá toàn bộ entity/dto/service/controller nghiệp vụ (Order, GiftCard, Customer, Supplier, CashTransaction, …).
- Đã xoá `Jobs/ExchangeRateJob`, `ScriptSql/Procedure/*`, `Templates/LineTemplate.xlsx`.
- DbContext rename `OrderDebtDbContext` → `EduTrackDbContext`, chỉ còn 8 DbSet Core.
- `Enums.cs` chỉ còn `GenderEnums`.
- `CommonService`/`CommonController` chỉ còn 3 dropdown core (page, user, role).
- `JsonOrgObjectDto` đơn giản hoá còn `AppName`.
- Sidebar `iconMap` rút gọn về 5 entry core.
- Namespace `OrderDebt.API` → `EduTrack.API`.

## Build status

- Backend: 0 errors (chỉ nullable warnings).
- Frontend: build dev OK (bundle ~7 MB).
