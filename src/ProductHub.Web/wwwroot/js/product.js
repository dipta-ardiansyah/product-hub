// products.js - Product CRUD, pagination, filter, search, and sorting
let currentPage = 1;
const pageSize = 10;
let currentSortBy = 'createdAt';
let currentSortDir = 'desc';
let productToDeleteId = null;

document.addEventListener('DOMContentLoaded', () => {
    if (!Auth.isAuthenticated()) {
        window.location.href = '/Account/Login';
        return;
    }

    loadProducts();

    // Event listeners
    document.getElementById('filterForm').addEventListener('submit', (e) => {
        e.preventDefault();
        currentPage = 1;
        loadProducts();
    });

    document.getElementById('btnResetFilter').addEventListener('click', () => {
        document.getElementById('filterName').value = '';
        document.getElementById('filterMinPrice').value = '';
        document.getElementById('filterMaxPrice').value = '';
        currentPage = 1;
        loadProducts();
    });

    document.getElementById('btnAddNewProduct').addEventListener('click', () => {
        openProductModal();
    });

    document.getElementById('productForm').addEventListener('submit', handleSaveProduct);
    document.getElementById('btnConfirmDelete').addEventListener('click', handleConfirmDelete);

    // Sorting
    document.getElementById('sortName').addEventListener('click', () => toggleSort('name'));
    document.getElementById('sortPrice').addEventListener('click', () => toggleSort('price'));
    document.getElementById('sortCreatedAt').addEventListener('click', () => toggleSort('createdAt'));
});

function toggleSort(field) {
    if (currentSortBy === field) {
        currentSortDir = currentSortDir === 'asc' ? 'desc' : 'asc';
    } else {
        currentSortBy = field;
        currentSortDir = 'asc';
    }
    loadProducts();
}

async function loadProducts() {
    const tableBody = document.getElementById('productTableBody');
    const tableAlert = document.getElementById('tableAlert');
    tableAlert.classList.add('d-none');

    const name = document.getElementById('filterName').value.trim();
    const minPrice = document.getElementById('filterMinPrice').value.trim();
    const maxPrice = document.getElementById('filterMaxPrice').value.trim();

    const params = new URLSearchParams({
        page: currentPage,
        pageSize: pageSize,
        sortBy: currentSortBy,
        sortDir: currentSortDir
    });

    if (name) params.append('name', name);
    if (minPrice) params.append('minPrice', minPrice);
    if (maxPrice) params.append('maxPrice', maxPrice);

    try {
        const result = await Api.get(`/api/products?${params.toString()}`);
        renderTable(result.items);
        renderPagination(result);
    } catch (err) {
        tableAlert.textContent = err.message || 'Failed to load products.';
        tableAlert.classList.remove('d-none');
        tableBody.innerHTML = `<tr><td colspan="6" class="text-center py-4 text-danger"><i class="bi bi-exclamation-triangle me-2"></i>Failed to load products.</td></tr>`;
    }
}

function renderTable(products) {
    const tableBody = document.getElementById('productTableBody');

    if (!products || products.length === 0) {
        tableBody.innerHTML = `<tr><td colspan="6" class="text-center py-5 text-muted"><i class="bi bi-inbox fs-3 d-block mb-2"></i>No products found.</td></tr>`;
        return;
    }

    tableBody.innerHTML = products.map(p => `
        <tr>
            <td class="fw-semibold text-secondary">#${p.id}</td>
            <td class="fw-bold">${Auth.escapeHtml(p.name)}</td>
            <td class="text-muted text-truncate" style="max-width: 250px;">${Auth.escapeHtml(p.description || '-')}</td>
            <td class="fw-semibold text-success">$${Number(p.price).toFixed(2)}</td>
            <td class="text-muted small">${new Date(p.createdAt).toLocaleString()}</td>
            <td class="text-end">
                <button class="btn btn-outline-primary btn-sm me-1" onclick="editProduct(${p.id})">
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-outline-danger btn-sm" onclick="confirmDelete(${p.id}, '${Auth.escapeHtml(p.name).replace(/'/g, "\\'")}')">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');
}

function renderPagination(data) {
    const info = document.getElementById('paginationInfo');
    const controls = document.getElementById('paginationControls');

    const start = data.totalCount === 0 ? 0 : (data.page - 1) * data.pageSize + 1;
    const end = Math.min(data.page * data.pageSize, data.totalCount);
    info.textContent = `Showing ${start} to ${end} of ${data.totalCount} entries`;

    if (data.totalPages <= 1) {
        controls.innerHTML = '';
        return;
    }

    let html = `
        <li class="page-item ${!data.hasPreviousPage ? 'disabled' : ''}">
            <button class="page-link" onclick="goToPage(${data.page - 1})" aria-label="Previous">&laquo;</button>
        </li>
    `;

    for (let i = 1; i <= data.totalPages; i++) {
        html += `
            <li class="page-item ${i === data.page ? 'active' : ''}">
                <button class="page-link" onclick="goToPage(${i})">${i}</button>
            </li>
        `;
    }

    html += `
        <li class="page-item ${!data.hasNextPage ? 'disabled' : ''}">
            <button class="page-link" onclick="goToPage(${data.page + 1})" aria-label="Next">&raquo;</button>
        </li>
    `;

    controls.innerHTML = html;
}

function goToPage(page) {
    currentPage = page;
    loadProducts();
}

function openProductModal(product = null) {
    const modalLabel = document.getElementById('productModalLabel');
    const alertBox = document.getElementById('modalAlert');
    alertBox.classList.add('d-none');

    document.getElementById('productId').value = product ? product.id : '';
    document.getElementById('productName').value = product ? product.name : '';
    document.getElementById('productPrice').value = product ? product.price : '';
    document.getElementById('productDescription').value = product ? product.description : '';

    modalLabel.textContent = product ? 'Edit Product' : 'Add Product';

    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('productModal'));
    modal.show();
}

async function editProduct(id) {
    try {
        const product = await Api.get(`/api/products/${id}`);
        openProductModal(product);
    } catch (err) {
        alert('Failed to retrieve product details: ' + err.message);
    }
}

async function handleSaveProduct(e) {
    e.preventDefault();
    const alertBox = document.getElementById('modalAlert');
    alertBox.classList.add('d-none');

    const id = document.getElementById('productId').value;
    const name = document.getElementById('productName').value.trim();
    const price = parseFloat(document.getElementById('productPrice').value);
    const description = document.getElementById('productDescription').value.trim();

    const payload = {
        name,
        price,
        description
    };

    try {
        if (id) {
            payload.id = parseInt(id, 10);
            await Api.put(`/api/products/${id}`, payload);
        } else {
            await Api.post('/api/products', payload);
        }

        const modal = bootstrap.Modal.getInstance(document.getElementById('productModal'));
        if (modal) modal.hide();

        loadProducts();
    } catch (err) {
        alertBox.textContent = err.message || 'Failed to save product.';
        alertBox.classList.remove('d-none');
    }
}

function confirmDelete(id, name) {
    productToDeleteId = id;
    document.getElementById('deleteProductName').textContent = name;
    document.getElementById('deleteAlert').classList.add('d-none');
    const modal = bootstrap.Modal.getOrCreateInstance(document.getElementById('deleteModal'));
    modal.show();
}

async function handleConfirmDelete() {
    if (!productToDeleteId) return;

    const alertBox = document.getElementById('deleteAlert');
    alertBox.classList.add('d-none');

    try {
        await Api.delete(`/api/products/${productToDeleteId}`);
        const modal = bootstrap.Modal.getInstance(document.getElementById('deleteModal'));
        if (modal) modal.hide();
        productToDeleteId = null;
        loadProducts();
    } catch (err) {
        alertBox.textContent = err.message || 'Failed to delete product.';
        alertBox.classList.remove('d-none');
    }
}
