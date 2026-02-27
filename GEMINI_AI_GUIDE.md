# ?? AI Tutor Chatbot - Gemini Integration

## ? HOÀN THÀNH

?ã tích h?p Google Gemini AI Chatbot vào StudyMate:
- ? AI Chatbot v?i context conversation
- ? Duy trì ng? c?nh cu?c h?i tho?i
- ? Build thành công

---

## ?? Gemini API Key

**API Key:** `AIzaSyBA_Ruv0yFfGXbpiVwq7XJuKeIz1dGTQm4`

?ã ???c c?u hình trong `appsettings.json`:
```json
{
  "GeminiAI": {
    "ApiKey": "AIzaSyBA_Ruv0yFfGXbpiVwq7XJuKeIz1dGTQm4"
  }
}
```

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

## ?? Cách S? D?ng Trong Frontend

### **React Component - AI Chatbot**

```javascript
import React, { useState, useEffect, useRef } from 'react';

function AIChatbot({ token }) {
    const [messages, setMessages] = useState([]);
    const [inputMessage, setInputMessage] = useState('');
    const [conversationId, setConversationId] = useState(null);
    const [loading, setLoading] = useState(false);
    const messagesEndRef = useRef(null);

    const scrollToBottom = () => {
        messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
    };

    useEffect(scrollToBottom, [messages]);

    async function sendMessage() {
        if (!inputMessage.trim()) return;

        // Add user message to UI
        const userMessage = {
            role: 'user',
            content: inputMessage,
            timestamp: new Date()
        };
        setMessages(prev => [...prev, userMessage]);
        setInputMessage('');
        setLoading(true);

        try {
            const response = await fetch('/api/aitutor/chat', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({
                    message: inputMessage,
                    conversationId: conversationId  // Dùng ID c? ?? duy trì context
                })
            });

            const result = await response.json();

            if (response.ok) {
                // L?u conversation ID ?? dùng cho câu h?i ti?p theo
                setConversationId(result.data.conversationId);

                // Add AI response to UI
                const aiMessage = {
                    role: 'assistant',
                    content: result.data.response,
                    timestamp: new Date(result.data.timestamp)
                };
                setMessages(prev => [...prev, aiMessage]);
            } else {
                alert('Error: ' + result.message);
            }
        } catch (error) {
            alert('Error connecting to AI: ' + error.message);
        } finally {
            setLoading(false);
        }
    }

    function handleKeyPress(e) {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            sendMessage();
        }
    }

    function startNewConversation() {
        setMessages([]);
        setConversationId(null);  // Reset = cu?c h?i tho?i m?i
    }

    return (
        <div className="ai-chatbot">
            <div className="chat-header">
                <h2>?? AI Tutor</h2>
                <button onClick={startNewConversation}>
                    New Conversation
                </button>
            </div>

            <div className="chat-messages">
                {messages.length === 0 && (
                    <div className="welcome-message">
                        <p>?? Hi! I'm your AI tutor. Ask me anything about your studies!</p>
                        <div className="suggestions">
                            <button onClick={() => setInputMessage("Explain recursion")}>
                                Explain recursion
                            </button>
                            <button onClick={() => setInputMessage("Help me with calculus")}>
                                Help with calculus
                            </button>
                            <button onClick={() => setInputMessage("What is machine learning?")}>
                                What is ML?
                            </button>
                        </div>
                    </div>
                )}

                {messages.map((msg, index) => (
                    <div key={index} className={`message ${msg.role}`}>
                        <div className="message-avatar">
                            {msg.role === 'user' ? '??' : '??'}
                        </div>
                        <div className="message-content">
                            <div className="message-text">{msg.content}</div>
                            <div className="message-time">
                                {msg.timestamp.toLocaleTimeString()}
                            </div>
                        </div>
                    </div>
                ))}

                {loading && (
                    <div className="message assistant">
                        <div className="message-avatar">??</div>
                        <div className="message-content">
                            <div className="typing-indicator">
                                <span></span><span></span><span></span>
                            </div>
                        </div>
                    </div>
                )}

                <div ref={messagesEndRef} />
            </div>

            <div className="chat-input">
                <textarea
                    value={inputMessage}
                    onChange={(e) => setInputMessage(e.target.value)}
                    onKeyPress={handleKeyPress}
                    placeholder="Ask me anything..."
                    rows="2"
                    disabled={loading}
                />
                <button 
                    onClick={sendMessage}
                    disabled={loading || !inputMessage.trim()}
                >
                    {loading ? 'Sending...' : 'Send'}
                </button>
            </div>
        </div>
    );
}

export default AIChatbot;
```

### **CSS (Optional)**

```css
.ai-chatbot {
    display: flex;
    flex-direction: column;
    height: 600px;
    max-width: 800px;
    margin: 0 auto;
    border: 1px solid #ddd;
    border-radius: 10px;
    overflow: hidden;
}

.chat-header {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
    color: white;
    padding: 15px 20px;
    display: flex;
    justify-content: space-between;
    align-items: center;
}

.chat-header h2 {
    margin: 0;
    font-size: 20px;
}

.chat-header button {
    background: rgba(255, 255, 255, 0.2);
    color: white;
    border: none;
    padding: 8px 15px;
    border-radius: 5px;
    cursor: pointer;
}

.chat-messages {
    flex: 1;
    overflow-y: auto;
    padding: 20px;
    background: #f5f5f5;
}

.message {
    display: flex;
    margin-bottom: 15px;
    animation: slideIn 0.3s ease;
}

@keyframes slideIn {
    from {
        opacity: 0;
        transform: translateY(10px);
    }
    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.message.user {
    flex-direction: row-reverse;
}

.message-avatar {
    font-size: 30px;
    margin: 0 10px;
}

.message-content {
    max-width: 70%;
    padding: 10px 15px;
    border-radius: 10px;
    background: white;
    box-shadow: 0 2px 5px rgba(0,0,0,0.1);
}

.message.user .message-content {
    background: #667eea;
    color: white;
}

.message-text {
    white-space: pre-wrap;
    line-height: 1.5;
}

.message-time {
    font-size: 11px;
    color: #999;
    margin-top: 5px;
}

.message.user .message-time {
    color: rgba(255,255,255,0.7);
}

.typing-indicator {
    display: flex;
    gap: 5px;
}

.typing-indicator span {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    background: #667eea;
    animation: bounce 1.4s infinite ease-in-out;
}

.typing-indicator span:nth-child(1) {
    animation-delay: -0.32s;
}

.typing-indicator span:nth-child(2) {
    animation-delay: -0.16s;
}

@keyframes bounce {
    0%, 80%, 100% {
        transform: scale(0);
    }
    40% {
        transform: scale(1);
    }
}

.chat-input {
    display: flex;
    padding: 15px;
    background: white;
    border-top: 1px solid #ddd;
}

.chat-input textarea {
    flex: 1;
    padding: 10px;
    border: 1px solid #ddd;
    border-radius: 5px;
    resize: none;
    font-family: inherit;
}

.chat-input button {
    margin-left: 10px;
    padding: 10px 25px;
    background: #667eea;
    color: white;
    border: none;
    border-radius: 5px;
    cursor: pointer;
    font-weight: bold;
}

.chat-input button:disabled {
    background: #ccc;
    cursor: not-allowed;
}

.welcome-message {
    text-align: center;
    padding: 40px 20px;
}

.suggestions {
    display: flex;
    gap: 10px;
    justify-content: center;
    margin-top: 20px;
    flex-wrap: wrap;
}

.suggestions button {
    padding: 10px 20px;
    background: white;
    border: 2px solid #667eea;
    color: #667eea;
    border-radius: 20px;
    cursor: pointer;
    transition: all 0.3s;
}

.suggestions button:hover {
    background: #667eea;
    color: white;
}
```

---

## ?? Technical Details

### **API Endpoint:**
```
POST https://generativelanguage.googleapis.com/v1beta/models/gemini-pro:generateContent?key={API_KEY}
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

---

## ?? Tóm T?t

### ? Tính N?ng:
- **AI Chatbot:** Trò chuy?n thông minh v?i context

### ?? API Endpoint:
- `POST /api/aitutor/chat` - Chat v?i AI Tutor

### ?? Files:
1. `Service\DTOs\ChatDto.cs` - Request/Response DTOs
2. `Service\Interface\IGeminiAIService.cs` - Service interface
3. `Service\Implement\GeminiAIService.cs` - Service implementation
4. `StudyMate_BE\Controllers\AITutorController.cs` - API endpoint
5. `StudyMate_BE\appsettings.json` - API key config

### ?? Test:
1. Ch?y project: `dotnet run --project StudyMate_BE`
2. M? Swagger: `https://localhost:7259/swagger`
3. Login ?? l?y token
4. Test endpoint `/api/aitutor/chat`

---

**AI Chatbot s?n sàng! ????**
