# API Documentation

## Base URL
```
http://localhost:5050/api/v1
```

## Authentication
Most endpoints require authentication via ASP.NET Identity cookies. Login via `/Account/Login` endpoint.

## Response Format
All responses follow this structure:
```json
{
  "data": [...],
  "meta": {
    "currentPage": 1,
    "totalPages": 10,
    "pageSize": 20,
    "totalCount": 195
  }
}
```

## Endpoints

### Facilities

#### List Facilities
```http
GET /api/v1/facilities
```

**Query Parameters:**
- `page` (int, optional) - Page number (default: 1)
- `pageSize` (int, optional) - Items per page (default: 20, max: 100)
- `search` (string, optional) - Search by name or description
- `city` (string, optional) - Filter by city
- `type` (int, optional) - Filter by facility type (0=Therapy, 1=School, 2=SupportCenter, 3=Clinic, 4=Other)
- `verificationStatus` (int, optional) - Filter by status (0=Pending, 1=Approved, 2=Rejected)

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Centrum Terapii ABC",
      "type": 0,
      "city": "Warsaw",
      "address": "ul. Przykładowa 1",
      "phoneNumber": "509-385-129",
      "email": "martgant@gmail.com",
      "website": "https://abc.pl",
      "description": "Opis placówki...",
      "verificationStatus": 1,
      "averageRating": 4.5,
      "reviewCount": 12,
      "createdAt": "2026-01-15T10:30:00Z"
    }
  ],
  "meta": {
    "currentPage": 1,
    "totalPages": 5,
    "pageSize": 20,
    "totalCount": 95
  }
}
```

#### Get Facility Details
```http
GET /api/v1/facilities/{id}
```

**Response:**
```json
{
  "id": 1,
  "name": "Centrum Terapii ABC",
  "type": 0,
  "city": "Warsaw",
  "postalCode": "00-001",
  "address": "ul. Przykładowa 1",
  "phoneNumber": "509-385-129",
  "email": "martgant@gmail.com",
  "website": "https://abc.pl",
  "description": "Pełny opis placówki...",
  "latitude": 52.2297,
  "longitude": 21.0122,
  "verificationStatus": 1,
  "averageRating": 4.5,
  "reviewCount": 12,
  "createdAt": "2026-01-15T10:30:00Z",
  "verifiedAt": "2026-01-16T14:20:00Z"
}
```

#### Get Facility Reviews
```http
GET /api/v1/facilities/{id}/reviews
```

**Query Parameters:**
- `page` (int, optional) - Page number
- `pageSize` (int, optional) - Items per page

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "facilityId": 1,
      "userId": 5,
      "userName": "Jan Kowalski",
      "rating": 5,
      "comment": "Świetna placówka, bardzo profesjonalny personel.",
      "isApproved": true,
      "createdAt": "2026-01-20T08:15:00Z"
    }
  ],
  "meta": {
    "currentPage": 1,
    "totalPages": 2,
    "pageSize": 10,
    "totalCount": 12
  }
}
```

### Reviews

#### Create Review
```http
POST /api/v1/reviews
```

**Authorization:** Required (User role)

**Request Body:**
```json
{
  "facilityId": 1,
  "rating": 5,
  "comment": "Bardzo dobra placówka, polecam!"
}
```

**Validation:**
- `rating`: Required, 1-5
- `comment`: Required, 10-1000 characters
- User must be authenticated
- User cannot review same facility twice

**Response (201 Created):**
```json
{
  "id": 15,
  "facilityId": 1,
  "rating": 5,
  "comment": "Bardzo dobra placówka, polecam!",
  "isApproved": false,
  "createdAt": "2026-01-27T10:30:00Z"
}
```

### Forum

#### List Categories
```http
GET /api/v1/forum/categories
```

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "name": "Ogólne",
      "slug": "ogolne",
      "description": "Ogólne dyskusje o autyzmie",
      "sortOrder": 1,
      "topicCount": 45,
      "postCount": 312
    }
  ]
}
```

#### Get Category Topics
```http
GET /api/v1/forum/categories/{id}/topics
```

**Query Parameters:**
- `page` (int) - Page number
- `pageSize` (int) - Items per page

**Response:**
```json
{
  "data": [
    {
      "id": 1,
      "categoryId": 1,
      "title": "Jak wybrać odpowiednią terapię?",
      "slug": "jak-wybrac-odpowiednia-terapie",
      "authorId": 5,
      "authorName": "Jan Kowalski",
      "postCount": 8,
      "viewCount": 152,
      "isPinned": false,
      "isLocked": false,
      "lastPostAt": "2026-01-26T15:30:00Z",
      "createdAt": "2026-01-20T10:00:00Z"
    }
  ],
  "meta": {...}
}
```

#### Get Topic with Posts
```http
GET /api/v1/forum/topics/{id}
```

**Query Parameters:**
- `page` (int) - Page for posts
- `pageSize` (int) - Posts per page

**Response:**
```json
{
  "topic": {
    "id": 1,
    "title": "Jak wybrać odpowiednią terapię?",
    "categoryId": 1,
    "authorName": "Jan Kowalski",
    "viewCount": 152,
    "isPinned": false,
    "isLocked": false,
    "createdAt": "2026-01-20T10:00:00Z"
  },
  "posts": {
    "data": [
      {
        "id": 1,
        "topicId": 1,
        "authorId": 5,
        "authorName": "Jan Kowalski",
        "content": "Treść pierwszego posta...",
        "isApproved": true,
        "createdAt": "2026-01-20T10:00:00Z",
        "editedAt": null
      }
    ],
    "meta": {...}
  }
}
```

#### Create Topic
```http
POST /api/v1/forum/topics
```

**Authorization:** Required

**Request Body:**
```json
{
  "categoryId": 1,
  "title": "Pytanie o terapię SI",
  "content": "Treść pierwszego posta..."
}
```

**Validation:**
- `title`: Required, 5-200 characters
- `content`: Required, 10-10000 characters
- Topic title must be unique within category

**Response (201 Created):**
```json
{
  "id": 25,
  "slug": "pytanie-o-terapie-si",
  "categoryId": 1,
  "title": "Pytanie o terapię SI",
  "createdAt": "2026-01-27T11:00:00Z"
}
```

#### Reply to Topic
```http
POST /api/v1/forum/topics/{id}/posts
```

**Authorization:** Required

**Request Body:**
```json
{
  "content": "Treść odpowiedzi..."
}
```

**Response (201 Created):**
```json
{
  "id": 45,
  "topicId": 25,
  "content": "Treść odpowiedzi...",
  "createdAt": "2026-01-27T11:05:00Z"
}
```

### Health Check

#### Get Health Status
```http
GET /health
```

**Response:**
```json
{
  "status": "healthy",
  "version": "1.0.0",
  "checks": {
    "database": "ok"
  },
  "timestamp": "2026-01-27T10:30:00Z"
}
```

## Error Responses

### 400 Bad Request
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "Rating": ["Rating must be between 1 and 5."],
    "Comment": ["Comment must be at least 10 characters long."]
  }
}
```

### 401 Unauthorized
```json
{
  "error": "Authentication required"
}
```

### 403 Forbidden
```json
{
  "error": "You do not have permission to access this resource"
}
```

### 404 Not Found
```json
{
  "error": "Resource not found"
}
```

### 429 Too Many Requests
```json
{
  "error": "Rate limit exceeded. Please try again later."
}
```

## Rate Limiting

API endpoints are rate-limited to prevent abuse:
- **General endpoints**: 100 requests per minute per IP
- **Authentication endpoints**: 10 requests per 5 minutes per IP
- **POST/PUT/DELETE**: 30 requests per minute per IP

Rate limit headers are included in responses:
```
X-Rate-Limit-Limit: 100
X-Rate-Limit-Remaining: 95
X-Rate-Limit-Reset: 1706349600
```

## Pagination

All list endpoints support pagination:
- Default page size: 20
- Maximum page size: 100
- Page numbers start at 1

Example:
```http
GET /api/v1/facilities?page=2&pageSize=50
```

## Filtering & Searching

### Search
Use `search` parameter for full-text search:
```http
GET /api/v1/facilities?search=terapia
```

### Filters
Multiple filters can be combined:
```http
GET /api/v1/facilities?city=Warsaw&type=0&verificationStatus=1
```

## CORS

CORS is enabled for all origins in development. In production, configure allowed origins in `appsettings.Production.json`.

## Swagger Documentation

Interactive API documentation available at:
```
http://localhost:5050/api/docs
```

## SDK / Client Libraries

Currently, no official SDKs are available. You can generate clients using:
- OpenAPI/Swagger specification
- Tools like NSwag, AutoRest, or openapi-generator

## Support

For API support or questions:
- Email: support@oaza.pl
- GitHub Issues: https://github.com/your-org/oaza-dla-autyzmu-dotnet/issues
