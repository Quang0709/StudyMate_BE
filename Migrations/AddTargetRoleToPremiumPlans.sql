-- Migration: Add target_role column to premium_plans table
-- Execute this script on your database

-- Step 1: Add target_role column
ALTER TABLE premium_plans
ADD target_role VARCHAR(20) NULL;

-- Step 2: Update existing data (if any)
-- Set default value for existing plans based on your business logic
-- Example: Set all existing plans to 'student'
UPDATE premium_plans
SET target_role = 'student'
WHERE target_role IS NULL;

-- Step 3: (Optional) Add check constraint to ensure valid values
ALTER TABLE premium_plans
ADD CONSTRAINT CK_PremiumPlan_TargetRole CHECK (target_role IN ('student', 'teacher'));

-- Step 4: Create some sample data (optional)
-- INSERT INTO premium_plans (name, duration_days, price, target_role) 
-- VALUES 
--     ('Student Basic', 30, 99000, 'student'),
--     ('Student Pro', 90, 249000, 'student'),
--     ('Student Premium', 365, 799000, 'student'),
--     ('Teacher Basic', 30, 149000, 'teacher'),
--     ('Teacher Pro', 90, 399000, 'teacher'),
--     ('Teacher Premium', 365, 1299000, 'teacher');

PRINT 'Migration completed successfully!';
