-- Create RefreshTokens table
CREATE TABLE refresh_tokens (
    id INT PRIMARY KEY IDENTITY(1,1),
    user_id INT NOT NULL,
    token NVARCHAR(500) NOT NULL,
    expiry_date DATETIME NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    is_revoked BIT DEFAULT 0,
    CONSTRAINT FK_RefreshToken_User FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

-- Create index for faster token lookup
CREATE INDEX IX_RefreshTokens_Token ON refresh_tokens(token);
CREATE INDEX IX_RefreshTokens_UserId ON refresh_tokens(user_id);

-- Create OtpCodes table
CREATE TABLE otp_codes (
    id INT PRIMARY KEY IDENTITY(1,1),
    email VARCHAR(255) NOT NULL,
    code VARCHAR(6) NOT NULL,
    expiry_date DATETIME NOT NULL,
    created_at DATETIME DEFAULT GETDATE(),
    is_used BIT DEFAULT 0
);

-- Create index for faster email lookup
CREATE INDEX IX_OtpCodes_Email ON otp_codes(email);
CREATE INDEX IX_OtpCodes_Code ON otp_codes(code);

GO
