CREATE DATABASE AromaDom_V6;
GO
USE AromaDom_V6;
GO

CREATE TABLE dbo.material_types (
    material_type_id INT PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    loss_percent DECIMAL(5,2) NOT NULL
);

CREATE TABLE dbo.product_types (
    product_type_id INT PRIMARY KEY,
    name NVARCHAR(100) NOT NULL UNIQUE,
    coefficient DECIMAL(6,2) NOT NULL
);

CREATE TABLE dbo.materials (
    material_id INT PRIMARY KEY,
    name NVARCHAR(150) NOT NULL UNIQUE,
    material_type_id INT NOT NULL REFERENCES dbo.material_types(material_type_id),
    price DECIMAL(12,2) NOT NULL,
    stock_quantity DECIMAL(12,3) NOT NULL,
    min_quantity DECIMAL(12,3) NOT NULL,
    package_quantity DECIMAL(12,3) NOT NULL,
    unit NVARCHAR(20) NOT NULL
);

CREATE TABLE dbo.products (
    product_id INT PRIMARY KEY,
    article NVARCHAR(30) NOT NULL UNIQUE,
    product_type_id INT NOT NULL REFERENCES dbo.product_types(product_type_id),
    name NVARCHAR(150) NOT NULL UNIQUE,
    min_price DECIMAL(12,2) NOT NULL
);

CREATE TABLE dbo.material_products (
    material_id INT NOT NULL REFERENCES dbo.materials(material_id),
    product_id INT NOT NULL REFERENCES dbo.products(product_id),
    quantity DECIMAL(12,3) NOT NULL,
    PRIMARY KEY (material_id, product_id)
);
