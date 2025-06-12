# API Type Generation

This project uses `openapi-typescript-codegen` to generate TypeScript types from the backend Swagger specification.

## Setup

The generated types are created in `app/api-generated/` as a sibling to the frontend app.

## Usage

### Generate Types

```bash
npm run generate-api
```

**Prerequisites:** The backend server must be running at `http://localhost:5015`

### Import Generated Types

```typescript
import { UsersService, AuctionsService, LotService } from '@/api-generated/services';
import { RegisterDto, LoginUserDto, AuctionDto } from '@/api-generated/models';
```

Or using relative imports:

```typescript
import { UsersService } from '../api-generated/services/UsersService';
import { RegisterDto } from '../api-generated/models/RegisterDto';
```

### Available Scripts

- `npm run generate-api` - Generate TypeScript types from Swagger
- `npm run generate-api:raw` - Run openapi-typescript-codegen directly

## Configuration

The generation script (`scripts/generate-api-types.js`) is configured to:
- Fetch from: `http://localhost:5015/swagger/v1/swagger.json`
- Output to: `../api-generated`
- Use axios as the HTTP client
