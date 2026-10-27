from pathlib import Path
from decimal import Decimal
from openpyxl import load_workbook

folder = Path(__file__).parent


def read_rows(name):
    file = next(file for file in (folder / 'data').glob('*.xlsx') if file.name.split('_', 1)[1] == name + '_import.xlsx')
    book = load_workbook(file, data_only=True)
    rows = list(book.active.values)[1:]
    book.close()
    return [row for row in rows if row[0] is not None]


def sql_value(value):
    if value is None:
        return 'NULL'
    if isinstance(value, str):
        return "N'" + value.replace("'", "''") + "'"
    return str(value)


def insert(table, rows):
    values = [ '(' + ', '.join(sql_value(value) for value in row) + ')' for row in rows ]
    return 'INSERT INTO dbo.' + table + ' VALUES\n' + ',\n'.join(values) + ';\n'


material_types = read_rows('Material_type')
product_types = read_rows('Product_type')
materials = read_rows('Materials')
products = read_rows('Products')
recipes = read_rows('Material_products')

material_type_ids = {row[0]: i for i, row in enumerate(material_types, 1)}
product_type_ids = {row[0]: i for i, row in enumerate(product_types, 1)}
material_ids = {row[0]: i for i, row in enumerate(materials, 1)}
product_ids = {row[2]: i for i, row in enumerate(products, 1)}

sql = (folder / 'schema.sql').read_text(encoding='utf-8')
sql += '\nSET XACT_ABORT ON;\nBEGIN TRANSACTION;\n'
sql += insert('material_types', [(i, name, Decimal(loss.replace('%', '').replace(',', '.'))) for i, (name, loss) in enumerate(material_types, 1)])
sql += insert('product_types', [(i, name, factor) for i, (name, factor) in enumerate(product_types, 1)])
sql += insert('materials', [(i, row[0], material_type_ids[row[1]], *row[2:]) for i, row in enumerate(materials, 1)])
sql += insert('products', [(i, row[0], product_type_ids[row[1]], row[2], row[3]) for i, row in enumerate(products, 1)])
sql += insert('material_products', [(material_ids[row[0]], product_ids[row[1]], row[2]) for row in recipes])
sql += 'COMMIT;\n'

output = folder / 'AromaDom.sql'
output.write_text(sql, encoding='utf-8-sig')
print('Готово:', output)
