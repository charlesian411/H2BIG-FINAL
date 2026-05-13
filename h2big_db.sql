CREATE DATABASE IF NOT EXISTS h2big_db;
USE h2big_db;

CREATE TABLE IF NOT EXISTS users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    fullname VARCHAR(255) NOT NULL,
    username VARCHAR(100) UNIQUE NOT NULL,
    password VARCHAR(255) NOT NULL,
    role ENUM('Admin', 'Staff', 'Rider') NOT NULL,
    status ENUM('Active', 'Inactive') DEFAULT 'Active',
    time_in DATETIME NULL,
    time_out DATETIME NULL
);

CREATE TABLE IF NOT EXISTS customers (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    contact VARCHAR(50),
    address TEXT,
    bottle_debt INT DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS products (
    id INT AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    price DECIMAL(10, 2) NOT NULL,
    stock INT DEFAULT 0
);

CREATE TABLE IF NOT EXISTS sales (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NULL,
    total_amount DECIMAL(10, 2) NOT NULL,
    type ENUM('Walk-In', 'Delivery') NOT NULL,
    date_time TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    staff_id INT NOT NULL,
    rider_id INT NULL,
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (staff_id) REFERENCES users(id),
    FOREIGN KEY (rider_id) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS sale_items (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sale_id INT NOT NULL,
    product_id INT NOT NULL,
    quantity INT NOT NULL,
    subtotal DECIMAL(10, 2) NOT NULL,
    FOREIGN KEY (sale_id) REFERENCES sales(id) ON DELETE CASCADE,
    FOREIGN KEY (product_id) REFERENCES products(id)
);

CREATE TABLE IF NOT EXISTS deliveries (
    id INT AUTO_INCREMENT PRIMARY KEY,
    sale_id INT NOT NULL,
    rider_id INT NOT NULL,
    status ENUM('Pending', 'Delivered', 'Cancelled') DEFAULT 'Pending',
    FOREIGN KEY (sale_id) REFERENCES sales(id),
    FOREIGN KEY (rider_id) REFERENCES users(id)
);

CREATE TABLE IF NOT EXISTS bottle_ledger (
    id INT AUTO_INCREMENT PRIMARY KEY,
    customer_id INT NOT NULL,
    bottles_out INT DEFAULT 0,
    bottles_in INT DEFAULT 0,
    balance INT DEFAULT 0,
    transaction_id INT NOT NULL,
    date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (transaction_id) REFERENCES sales(id)
);

CREATE TABLE IF NOT EXISTS expenses (
    id INT AUTO_INCREMENT PRIMARY KEY,
    description VARCHAR(255) NOT NULL,
    amount DECIMAL(10, 2) NOT NULL,
    date DATE NOT NULL
);

-- SEED DATA
-- Assuming simple plain text passwords for initialization. 
-- Will be upgraded to hashed passwords when implementing the C# auth module if required.
INSERT IGNORE INTO users (id, fullname, username, password, role) VALUES
(1, 'System Administrator', 'admin', 'admin123', 'Admin'),
(2, 'John Doe (Staff)', 'staff1', 'staff123', 'Staff'),
(3, 'Mike Rider', 'rider1', 'rider123', 'Rider');

INSERT IGNORE INTO products (id, name, price, stock) VALUES
(1, '5-Gallon Slim Container', 40.00, 100),
(2, '5-Gallon Round Container', 35.00, 150),
(3, '1-Gallon Container', 20.00, 50);

INSERT IGNORE INTO customers (id, name, contact, address, bottle_debt) VALUES
(1, 'Walk-In Customer', 'N/A', 'N/A', 0),
(2, 'Alice Smith', '09123456789', '123 Main St, Cityville', 2),
(3, 'Bob Johnson', '09876543210', '456 Elm St, Townsburg', 0);
