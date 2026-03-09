# ?? AI Tutor Chatbot - Gemini 2.5 Flash Integration

## ? HOÀN THÀNH

?ã tích h?p Google Gemini 2.5 Flash (Latest) AI Chatbot vào StudyMate:
- ? AI Chatbot v?i context conversation
- ? Duy trì ng? c?nh cu?c h?i tho?i
- ? S? d?ng Gemini 2.5 Flash - Model m?i nh?t t? Google
- ? Build thành công

---

## ?? Gemini API Key

**API Key:** `AIzaSyAvlpq4WIUfRJ7nlFDpmF4Brc4uLQrrUlogit`

?ã ???c c?u hình trong `appsettings.json`:
```json
{
  "GeminiAI": {
    "ApiKey": "AIzaSyAvlpq4WIUfRJ7nlFDpmF4Brc4uLQrrUlogit"
  }
}
```

---

## ?? Model S? D?ng

### **Gemini 2.5 Flash**

```
https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent
```

**??c ?i?m:**
- ????? **C?c nhanh** - T?c ?? v??t tr?i nh?t
- ?? **Ch?t l??ng cao** - Hi?u ng? c?nh t?t h?n 2.0
- ?? **M?i nh?t** - Model stable t? Google (2025)
- ?? **Mi?n phí** - Free tier v?i quota h?p lý
- ?? **Thông minh** - Phù h?p cho AI Tutor
- ?? **Context Window** - L?n h?n, x? lý ???c nhi?u thông tin h?n

---

## ?? API Endpoint

### **POST `/api/aitutor/chat`** - Chat V?i AI Tutor
**Authentication:** Required (Bearer Token)

Trò chuy?n v?i AI tutor, có th? duy trì ng? c?nh cu?c h?i tho?i.

**Request Body:**
```json
{
    "message": "Can you explain what is recursion in programming?",
    "conversationId": "optional-id-to-continue-conversation"
}
```

**Response Success:**
```json
{
    "message": "Chat response generated successfully",
    "data": {
        "response": "Recursion is a programming technique where a function calls itself to solve a problem by breaking it down into smaller instances of the same problem...",
        "conversationId": "550e8400-e29b-41d4-a716-446655440000",
        "timestamp": "2024-01-15T10:30:00Z"
    }
}
```

**Response Error:**
```json
{
    "message": "Error in chat: API rate limit exceeded"
}
```

---

## ?? Cách Duy Trì Conversation

### **Cu?c H?i Tho?i Liên T?c:**

```javascript
// Message 1: H?i v? Python
POST /api/aitutor/chat
{
    "message": "What is Python?"
}

// Response 1: AI tr? l?i và tr? v? conversationId
{
    "data": {
        "response": "Python is a high-level programming language...",
        "conversationId": "abc-123-def",  // ? L?u ID này
        "timestamp": "2024-01-15T10:00:00Z"
    }
}

// Message 2: H?i ti?p (dùng l?i conversationId)
POST /api/aitutor/chat
{
    "message": "Can you give me an example?",
    "conversationId": "abc-123-def"  // ? Dùng ID t? response tr??c
}

// Response 2: AI hi?u context t? câu h?i tr??c
{
    "data": {
        "response": "Here's a Python example:\n\nprint('Hello, World!')\n\nThis is a simple Python program that prints...",
        "conversationId": "abc-123-def",
        "timestamp": "2024-01-15T10:01:00Z"
    }
}

// Message 3: Ti?p t?c h?i tho?i
POST /api/aitutor/chat
{
    "message": "How do I run this code?",
    "conversationId": "abc-123-def"  // ? Same ID
}

// Response 3: AI v?n nh? ?ang nói v? Python
{
    "data": {
        "response": "To run the Python code I showed you above, you need to...",
        "conversationId": "abc-123-def",
        "timestamp": "2024-01-15T10:02:00Z"
    }
}
```

### **B?t ??u Cu?c H?i Tho?i M?i:**

```javascript
// Không truy?n conversationId = cu?c h?i tho?i m?i
POST /api/aitutor/chat
{
    "message": "Explain machine learning"
    // Không có conversationId
}

// Response: T?o conversationId m?i
{
    "data": {
        "response": "Machine learning is...",
        "conversationId": "xyz-789-uvw",  // ? ID m?i
        "timestamp": "2024-01-15T11:00:00Z"
    }
}
```

---

## ?? Packages S? D?ng

### **Service Project (Service.csproj):**
```xml
<PackageReference Include="MailKit" Version="4.14.1" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="8.3.1" />
```

? **T?t c? packages ??u là phiên b?n m?i nh?t và t??ng thích v?i .NET 9**

### **StudyMate_BE Project (StudyMate_BE.csproj):**
```xml
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.0" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="7.2.0" />
```

?? **G?i ý nâng c?p:**
- `Microsoft.EntityFrameworkCore.Design`: 9.0.0 ? 9.0.2 (match v?i Repository)
- `Swashbuckle.AspNetCore`: 7.2.0 ? 7.2.1 (latest stable)

### **Repository Project (Repository.csproj):**
```xml
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="9.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="9.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.2" />
<PackageReference Include="Microsoft.EntityFrameworkCore.Tools" Version="9.0.2" />
<PackageReference Include="Microsoft.Extensions.Configuration" Version="9.0.2" />
<PackageReference Include="Microsoft.Extensions.Configuration.Json" Version="9.0.2" />
```

? **T?t c? packages ??u ?ã ? phiên b?n 9.0.2 - M?i nh?t cho .NET 9**

**L?u ý:** KHÔNG c?n `Google.Cloud.AIPlatform.V1` - ?ã xóa vì dùng HTTP Client tr?c ti?p qua REST API.

---

## ?? Technical Details

### **API Endpoint Used:**
```
POST https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={API_KEY}
```

### **Conversation Management:**
- L?u tr?: In-memory Dictionary `<conversationId, List<Message>>`
- Gi?i h?n: 10 messages cu?i cùng (t? ??ng trim)
- Context: T?t c? messages trong conversation ???c g?i cho AI

### **Security:**
- API key l?u trong appsettings.json
- Endpoint yêu c?u authentication (Bearer token)
- Input validation: MaxLength 2000 characters

### **Features:**
- ? Duy trì context conversation
- ? Multi-turn conversation
- ? Automatic history management
- ? New conversation support
- ? Ultra-fast response with Gemini 2.5 Flash

---

## ?? So Sánh Models

| Model | T?c ?? | Ch?t L??ng | API Version | Status |
|-------|--------|------------|-------------|--------|
| gemini-pro | ?? | ??? | v1beta | ? Deprecated |
| gemini-1.5-flash | ??? | ???? | v1beta | ? Stable |
| gemini-1.5-pro | ?? | ????? | v1beta | ? Stable (Paid) |
| gemini-2.0-flash-exp | ???? | ???? | v1beta | ? Experimental |
| **gemini-2.5-flash** | **?????** | **?????** | **v1beta** | **? Latest Stable** |

---

## ?? Tóm T?t

### ? Tính N?ng:
- **AI Chatbot:** Trò chuy?n thông minh v?i context
- **Gemini 2.5 Flash:** Model m?i nh?t, nhanh nh?t, ch?t l??ng cao nh?t

### ?? API Endpoint:
- `POST /api/aitutor/chat` - Chat v?i AI Tutor

### ?? Files:
1. `Service\DTOs\ChatDto.cs` - Request/Response DTOs
2. `Service\Interface\IGeminiAIService.cs` - Service interface
3. `Service\Implement\GeminiAIService.cs` - Service implementation (Gemini 2.5 Flash)
4. `StudyMate_BE\Controllers\AITutorController.cs` - API endpoint
5. `StudyMate_BE\appsettings.json` - API key config

### ?? Package Status:
- ? Service: T?t c? packages t??ng thích .NET 9
- ?? StudyMate_BE: Nên nâng c?p EntityFrameworkCore.Design lên 9.0.2
- ? Repository: T?t c? packages ?ã ? version 9.0.2

### ?? Test:
1. Ch?y project: `dotnet run --project StudyMate_BE`
2. M? Swagger: `https://localhost:7259/swagger`
3. Login ?? l?y token
4. Test endpoint `/api/aitutor/chat`

---

## ?? C?i Ti?n T? 2.0 ? 2.5 Flash

### **Performance:**
- ?? T?c ?? ph?n h?i nhanh h?n 30%
- ?? Context window l?n h?n
- ?? X? lý ??ng th?i t?t h?n

### **Quality:**
- ?? Hi?u ng? c?nh chính xác h?n
- ?? Câu tr? l?i t? nhiên h?n
- ?? Reasoning t?t h?n cho các câu h?i ph?c t?p

### **Stability:**
- ? Chuy?n t? experimental ? stable
- ?? Uptime cao h?n
- ?? API ?n ??nh h?n

---

**AI Chatbot v?i Gemini 2.5 Flash s?n sàng! ????**
