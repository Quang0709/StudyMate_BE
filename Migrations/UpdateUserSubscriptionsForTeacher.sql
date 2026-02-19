-- Migration: Update user_subscriptions table to support both Student and Teacher
-- Execute this script on your database

-- Step 1: Add teacher_id column
ALTER TABLE user_subscriptions
ADD teacher_id INT NULL;

-- Step 2: Add is_active column
ALTER TABLE user_subscriptions
ADD is_active BIT NOT NULL DEFAULT 1;

-- Step 3: Make student_id nullable (allow NULL for teacher subscriptions)
ALTER TABLE user_subscriptions
ALTER COLUMN student_id INT NULL;

-- Step 4: Drop old foreign key constraint
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Sub_Student')
    ALTER TABLE user_subscriptions DROP CONSTRAINT FK_Sub_Student;

-- Step 5: Add new foreign key constraints
ALTER TABLE user_subscriptions
ADD CONSTRAINT FK_Sub_Student FOREIGN KEY (student_id) 
REFERENCES students(id) ON DELETE CASCADE;

ALTER TABLE user_subscriptions
ADD CONSTRAINT FK_Sub_Teacher FOREIGN KEY (teacher_id) 
REFERENCES teachers(id) ON DELETE CASCADE;

-- Step 6: Add check constraint to ensure either student_id or teacher_id is set (not both)
ALTER TABLE user_subscriptions
ADD CONSTRAINT CK_UserSubscription_OneRole 
CHECK (
    (student_id IS NOT NULL AND teacher_id IS NULL) 
    OR 
    (student_id IS NULL AND teacher_id IS NOT NULL)
);

-- Step 7: Create indexes for better query performance
CREATE INDEX IX_UserSubscriptions_TeacherId ON user_subscriptions(teacher_id);
CREATE INDEX IX_UserSubscriptions_IsActive ON user_subscriptions(is_active);
CREATE INDEX IX_UserSubscriptions_EndDate ON user_subscriptions(end_date);

PRINT 'Migration completed successfully!';
