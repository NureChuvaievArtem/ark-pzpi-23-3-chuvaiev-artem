INSERT INTO public."Roles"(
    "Name", "CreatedOn", "LastModifiedOn")
VALUES ('Client', NOW(), NOW());

INSERT INTO public."Roles"(
    "Name", "CreatedOn", "LastModifiedOn")
VALUES ('Courier', NOW(), NOW());

INSERT INTO public."PackageCategories"(
    "Name", "IsFragile", "CreatedOn", "LastModifiedOn")
VALUES
    ('Electronics', true, NOW(), NOW()),
    ('Clothing', false, NOW(), NOW()),
    ('Glassware', true, NOW(), NOW()),
    ('Books', false, NOW(), NOW()),
    ('Furniture', false, NOW(), NOW());

INSERT INTO public."DeliveryStatuses"(
    "Name", "CreatedOn", "LastModifiedOn")
VALUES
    ('Pending', NOW(), NOW()),
    ('In Progress', NOW(), NOW()),
    ('Delivered', NOW(), NOW()),
    ('Received', NOW(), NOW());

INSERT INTO public."Packages"(
    "UserId", "Height", "Width", "Depth", "PostBoxId", "CategoryId", "DeliveryStatusId", "CreatedOn", "LastModifiedOn")
VALUES
    (3,20.5, 15.0, 10.0, 1, 1, 1, NOW(), NOW()),
    (3,30.0, 25.0, 12.5, 2, 2, 1, NOW(), NOW()),
    (3,15.0, 10.0, 8.0, 1, 3, 2, NOW(), NOW());