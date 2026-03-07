// products.js - lightweight product list + cart logic
// - Fetch /api/products
// - Render products with stock badges
// - Cart kept in localStorage; client-side checks vs stock

let products = [];

// Read cart from multiple possible localStorage keys and normalize items
function readCartFromStorage() {
  const keys = ['cart', 'Cart', 'shoppingCart', 'cartItems', 'userCart'];
  for (const k of keys) {
    const v = localStorage.getItem(k);
    if (!v) continue;
    try {
      const parsed = JSON.parse(v);
      const arr = parsed && parsed.items ? parsed.items : parsed;
      if (!Array.isArray(arr)) continue;
      const normalized = arr.map(it => {
        if (it == null) return null;
        if (typeof it === 'number' || typeof it === 'string') return { id: Number(it), quantity: 1 };
        if (typeof it === 'object') {
          const id = it.id ?? it.Id ?? it.productId ?? it.productID ?? null;
          const quantity = it.quantity ?? it.Quantity ?? it.qty ?? 1;
          if (id == null) return null;
          return { id: Number(id), quantity: Number(quantity) || 1 };
        }
        return null;
      }).filter(x => x && x.id != null && x.id > 0 && !isNaN(x.id)); // Filter invalid IDs (null, 0, negative, NaN)
      if (normalized.length > 0) return normalized;
    } catch (e) {}
  }
  return [];
}

let cart = readCartFromStorage();

function escapeHtml(s) {
  if (!s) return '';
  return String(s).replace(/[&<>"']/g, c => ({ '&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;' })[c]);
}

function parsePrice(priceStr) {
  if (!priceStr) return 0;
  return parseInt(priceStr.replace(/[^0-9]/g, '')) || 0;
}

function formatPrice(value) {
  return value.toLocaleString('vi-VN') + '₫';
}

window.productsLoading = false;
window.productsLoadError = null;
let currentPage = 1, currentPageSize = 8, currentCategory = 'all', currentSort = '', currentSearch = '';

async function loadProducts(page = 1, pageSize = 8, category = 'all', sort = '', search = '') {
  try {
    window.productsLoading = true;
    window.productsLoadError = null;
    currentPage = page; currentPageSize = pageSize; currentCategory = category; currentSort = sort; currentSearch = search;

    const params = new URLSearchParams();
    if (category) params.set('category', category);
    if (search) params.set('search', search);
    if (sort) params.set('sort', sort);
    if (pageSize && pageSize > 0) params.set('pageSize', pageSize);
    if (page && page > 0) params.set('page', page);

    const resp = await fetch('/api/products?' + params.toString());
    if (!resp.ok) {
      const txt = await resp.text().catch(()=>'');
      window.productsLoadError = `API ${resp.status}: ${txt}`;
      renderProducts();
      return;
    }

    const data = await resp.json();
    products = data.items || data || [];

    const countEl = document.getElementById('productsCount');
    if (countEl) {
      const total = (data && (data.total || (Array.isArray(data) ? data.length : (data.items ? data.items.length : 0)))) || products.length || 0;
      countEl.textContent = `${total} sản phẩm`;
    }

    renderProducts(); updateCartBadge(); renderCart();
    renderPagination(data.total || products.length, data.page || page, data.pageSize || pageSize);
  } catch (err) {
    window.productsLoadError = err && err.message ? err.message : String(err);
    try { renderProducts(); } catch(e){}
  } finally {
    window.productsLoading = false;
  }
}

function renderProducts() {
  const container = document.getElementById('productsContainer'); if (!container) return;
  const loadingEl = container.querySelector('.products-loading'); if (loadingEl) loadingEl.remove();
  container.innerHTML = '';

  if (!products || products.length === 0) {
    if (window.productsLoading) {
      container.innerHTML = `<div class="col-12 text-center py-5"><div class="spinner-border text-primary mb-3" style="width: 3rem; height: 3rem;" role="status"><span class="visually-hidden">Đang tải...</span></div><p class="text-muted fw-semibold">Đang tải sản phẩm...</p></div>`;
      return;
    }
    if (window.productsLoadError) {
      container.innerHTML = `<div class="col-12 p-4 text-center text-danger"><i class="fas fa-exclamation-circle fa-3x mb-3"></i><p>Lỗi khi tải sản phẩm: ${escapeHtml(window.productsLoadError)}</p></div>`;
      return;
    }
    container.innerHTML = `<div class="col-12 p-4 text-center text-muted"><i class="fas fa-box-open fa-3x mb-3 text-muted opacity-50"></i><p class="fw-semibold">Không có sản phẩm để hiển thị.</p></div>`;
    return;
  }

  for (const p of products) {
    const isDiscount = p.originalPrice && p.discount;
    const stock = p.stock ?? p.StockQuantity ?? 0;
    const isNew = p.createdAt ? (new Date() - new Date(p.createdAt)) < (7 * 24 * 60 * 60 * 1000) : false; // New if < 7 days
    const isLowStock = stock > 0 && stock < 10;

    // Badges
    let badges = '';
    if (isDiscount && p.discount > 0) {
      badges += `<div class="position-absolute top-0 start-0 m-2"><span class="badge bg-danger shadow-sm px-3 py-2">${p.discount}% OFF</span></div>`;
    }
    if (isNew) {
      badges += `<div class="position-absolute top-0 end-0 m-2"><span class="badge bg-success shadow-sm px-2 py-1"><i class="fas fa-star me-1"></i>NEW</span></div>`;
    }
    if (isLowStock) {
      badges += `<div class="position-absolute bottom-0 end-0 m-2"><span class="badge bg-warning text-dark shadow-sm px-2 py-1"><i class="fas fa-fire me-1"></i>Sắp hết</span></div>`;
    }

    const priceHTML = isDiscount 
      ? `<div class="d-flex flex-column"><span class="text-decoration-line-through text-muted small">${p.originalPrice}</span><span class="text-danger fw-bold fs-5">${p.price}</span></div>` 
      : `<span class="text-primary fw-bold fs-5">${p.price}</span>`;

    const stockBadge = (stock === 0) 
      ? '<span class="badge bg-secondary">Hết hàng</span>' 
      : (stock < 5 
        ? `<span class="badge bg-warning text-dark">Còn ${stock}</span>` 
        : `<span class="badge bg-success">Còn ${stock}</span>`);

    const disabled = (stock === 0) ? 'disabled' : '';

    // Ẩn nút "Thêm vào giỏ" cho admin
    const adminHidden = isAdmin() ? 'style="display:none;"' : '';
    const overlayHidden = isAdmin() ? 'style="display:none !important;"' : '';

    const card = `
      <div class="col product-card" data-product-id="${p.id}" data-category="${p.category}">
        <div class="card h-100 border-0 shadow-sm product-card-hover product-card-clickable position-relative overflow-hidden" style="cursor: pointer;">
          ${badges}
          <div class="product-image-wrapper position-relative overflow-hidden">
            <div class="ratio ratio-4x3">
              <img src="${p.image}" class="card-img-top product-img" alt="${escapeHtml(p.name)}" loading="lazy" decoding="async">
            </div>
            <div class="product-overlay position-absolute w-100 h-100 top-0 start-0 d-flex align-items-center justify-content-center" ${overlayHidden}>
              <button type="button" class="btn btn-light btn-lg rounded-circle shadow btn-add-to-cart-overlay" onclick="event.stopPropagation(); addToCart(${p.id})" aria-label="Thêm vào giỏ hàng" ${disabled}>
                <i class="fas fa-cart-plus"></i>
              </button>
            </div>
          </div>
          <div class="card-body d-flex flex-column p-3">
            <h5 class="card-title text-truncate mb-2" title="${escapeHtml(p.name)}">${escapeHtml(p.name)}</h5>
            <p class="card-text text-muted small mb-3" style="display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; overflow: hidden;">${escapeHtml(p.desc)}</p>
            <div class="mt-auto">
              <div class="d-flex align-items-center justify-content-between mb-2">
                ${priceHTML}
                ${stockBadge}
              </div>
              <button type="button" class="btn btn-primary w-100 btn-sm shadow-sm btn-add-to-cart" onclick="event.stopPropagation(); addToCart(${p.id})" ${disabled} ${adminHidden}>
                <i class="fas fa-shopping-cart me-2"></i>Thêm vào giỏ
              </button>
            </div>
          </div>
        </div>
      </div>`;

    container.insertAdjacentHTML('beforeend', card);
  }

  // Thêm click handler cho các product card
  attachProductCardClickHandlers();
}

// Function to attach click handlers to product cards
function attachProductCardClickHandlers() {
  const productCards = document.querySelectorAll('.product-card-clickable');
  productCards.forEach(card => {
    card.addEventListener('click', function(e) {
      // Không mở modal nếu click vào nút thêm vào giỏ hàng
      if (e.target.closest('.btn-add-to-cart') || e.target.closest('.btn-add-to-cart-overlay')) {
        return;
      }
      const productId = parseInt(this.closest('.product-card').getAttribute('data-product-id'));
      if (!isNaN(productId)) {
        showProductDetailModal(productId);
      }
    });
  });
}

// Function to check if user is admin
function isAdmin() {
  return window.userRole && window.userRole.toLowerCase() === 'admin';
}

// Function to show product detail modal
function showProductDetailModal(productId) {
  const product = products.find(p => p.id === productId);
  if (!product) return;

  // Set product name
  document.getElementById('modalProductName').textContent = product.name || '';

  // Set product image
  const imgEl = document.getElementById('modalProductImage');
  imgEl.src = product.image || '';
  imgEl.alt = product.name || '';

  // Set badges
  const badgesContainer = document.getElementById('modalProductBadges');
  badgesContainer.innerHTML = '';

  const stock = product.stock ?? product.StockQuantity ?? 0;
  const isNew = product.createdAt ? (new Date() - new Date(product.createdAt)) < (7 * 24 * 60 * 60 * 1000) : false;

  if (product.discount && product.discount > 0) {
    badgesContainer.innerHTML += `<span class="badge bg-danger shadow-sm px-3 py-2 mb-1 d-block">${product.discount}% OFF</span>`;
  }
  if (isNew) {
    badgesContainer.innerHTML += `<span class="badge bg-success shadow-sm px-2 py-1 mb-1 d-block"><i class="fas fa-star me-1"></i>NEW</span>`;
  }

  // Set prices
  const originalPriceEl = document.getElementById('modalOriginalPrice');
  const currentPriceEl = document.getElementById('modalCurrentPrice');
  const discountBadgeEl = document.getElementById('modalDiscountBadge');

  if (product.originalPrice && product.discount) {
    originalPriceEl.textContent = product.originalPrice;
    originalPriceEl.style.display = 'inline';
    currentPriceEl.textContent = product.price;
    currentPriceEl.classList.add('text-danger');
    currentPriceEl.classList.remove('text-primary');
    discountBadgeEl.textContent = `-${product.discount}%`;
    discountBadgeEl.style.display = 'inline-block';
  } else {
    originalPriceEl.style.display = 'none';
    currentPriceEl.textContent = product.price;
    currentPriceEl.classList.add('text-primary');
    currentPriceEl.classList.remove('text-danger');
    discountBadgeEl.style.display = 'none';
  }

  // Set stock status
  const stockStatusEl = document.getElementById('modalStockStatus');
  if (stock === 0) {
    stockStatusEl.textContent = 'Hết hàng';
    stockStatusEl.className = 'badge bg-secondary';
  } else if (stock < 5) {
    stockStatusEl.textContent = `Còn ${stock} sản phẩm`;
    stockStatusEl.className = 'badge bg-warning text-dark';
  } else if (stock < 10) {
    stockStatusEl.textContent = `Còn ${stock} sản phẩm`;
    stockStatusEl.className = 'badge bg-info';
  } else {
    stockStatusEl.textContent = `Còn ${stock} sản phẩm`;
    stockStatusEl.className = 'badge bg-success';
  }

  // Set description
  document.getElementById('modalProductDescription').textContent = product.desc || product.description || 'Chưa có mô tả';

  // Set long description
  const longDescSection = document.getElementById('modalLongDescriptionSection');
  const longDescEl = document.getElementById('modalProductLongDescription');
  if (product.longDescription || product.longDesc) {
    longDescEl.textContent = product.longDescription || product.longDesc;
    longDescSection.style.display = 'block';
  } else {
    longDescSection.style.display = 'none';
  }

  // Set specifications
  const specsSection = document.getElementById('modalSpecificationsSection');
  const specsEl = document.getElementById('modalProductSpecifications');
  if (product.specifications || product.specs) {
    const specs = product.specifications || product.specs;
    if (typeof specs === 'string') {
      // If specs is a string, try to format it nicely
      const lines = specs.split('\n').filter(line => line.trim());
      specsEl.innerHTML = '<ul class="mb-0">' + lines.map(line => `<li>${escapeHtml(line)}</li>`).join('') + '</ul>';
    } else if (typeof specs === 'object') {
      // If specs is an object, display as key-value pairs
      specsEl.innerHTML = '<ul class="mb-0">' + Object.entries(specs).map(([key, value]) => `<li><strong>${escapeHtml(key)}:</strong> ${escapeHtml(value)}</li>`).join('') + '</ul>';
    } else {
      specsEl.textContent = String(specs);
    }
    specsSection.style.display = 'block';
  } else {
    specsSection.style.display = 'none';
  }

  // Set add to cart button state - Ẩn với admin
  const addToCartBtn = document.getElementById('modalAddToCartBtn');
  addToCartBtn.setAttribute('data-product-id', productId);

  // Kiểm tra admin - ẩn nút thêm vào giỏ
  if (isAdmin()) {
    addToCartBtn.style.display = 'none';
  } else {
    addToCartBtn.style.display = 'inline-block';
    if (stock === 0) {
      addToCartBtn.disabled = true;
      addToCartBtn.innerHTML = '<i class="fas fa-ban me-2"></i>Hết hàng';
    } else {
      addToCartBtn.disabled = false;
      addToCartBtn.innerHTML = '<i class="fas fa-cart-plus me-2"></i>Thêm vào giỏ hàng';
    }
  }

  // Show modal
  const modal = new bootstrap.Modal(document.getElementById('productDetailModal'));
  modal.show();
}

// Function to add to cart from modal
function addToCartFromModal() {
  // Chặn admin thêm vào giỏ
  if (isAdmin()) {
    if (window.siteEnhancements && window.siteEnhancements.showNotification) {
      window.siteEnhancements.showNotification('Tài khoản Admin không thể thêm sản phẩm vào giỏ hàng', 'warning');
    }
    return;
  }

  const addToCartBtn = document.getElementById('modalAddToCartBtn');
  const productId = parseInt(addToCartBtn.getAttribute('data-product-id'));
  if (!isNaN(productId)) {
    addToCart(productId);
  }
}

function addToCart(productId) {
  // Chặn admin thêm vào giỏ
  if (isAdmin()) {
    if (window.siteEnhancements && window.siteEnhancements.showNotification) {
      window.siteEnhancements.showNotification('Tài khoản Admin không thể thêm sản phẩm vào giỏ hàng', 'warning');
    }
    return;
  }

  const prod = (products || []).find(p => p.id === productId);
  if (!prod) return;
  const stock = prod.stock ?? prod.StockQuantity ?? 0;
  const existing = cart.find(i => i.id === productId);
  if (existing) {
    if (existing.quantity + 1 > stock) {
      if (window.siteEnhancements && window.siteEnhancements.showNotification) window.siteEnhancements.showNotification(`Không thể thêm: chỉ còn ${stock} trong kho`, 'warning');
      return;
    }
    existing.quantity++;
  } else {
    if (stock <= 0) { if (window.siteEnhancements && window.siteEnhancements.showNotification) window.siteEnhancements.showNotification('Sản phẩm đã hết hàng', 'warning'); return; }
    cart.push({ id: productId, quantity: 1 });
  }
  localStorage.setItem('cart', JSON.stringify(cart)); updateCartBadge(); renderCart();
  if (window.siteEnhancements && window.siteEnhancements.showNotification) window.siteEnhancements.showNotification(`${prod.name} đã được thêm vào giỏ`, 'success');
}

function updateCartBadge() { 
  const count = cart.reduce((s,i)=>s+i.quantity,0); 
  const badge = document.getElementById('cartCount'); 
  if (badge) { 
    if (count > 0) {
      badge.style.display = 'inline-block';
      badge.textContent = count;
    } else {
      badge.style.display = 'none';
      badge.textContent = '0';
    }
  } 
}

function renderCart() {
  const container = document.getElementById('cartItems'); const emptyMsg = document.getElementById('cartEmpty'); const totalEl = document.getElementById('cartTotal');
  if (!container) return;

  if (!products || products.length === 0) {
    if (window.productsLoading) {
      container.innerHTML = `<div class="d-flex align-items-center justify-content-center" style="min-height:160px;"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">Đang tải...</span></div></div>`;
      return;
    }
    if (window.productsLoadError) {
      container.innerHTML = `<div class="p-3 text-center text-danger">Lỗi khi tải sản phẩm: ${escapeHtml(window.productsLoadError)}</div>`;
      return;
    }
    container.innerHTML = `<div class="p-3 text-center text-muted">Không có sản phẩm để hiển thị.</div>`;
    return;
  }

  // Clean invalid items from cart (id <= 0, NaN, or product not found)
  const validCart = cart.filter(item => {
    if (!item || !item.id || item.id <= 0 || isNaN(item.id)) return false;
    const prod = products.find(p => p.id === item.id);
    return prod != null;
  });

  // Update cart if invalid items were removed
  if (validCart.length !== cart.length) {
    cart = validCart;
    localStorage.setItem('cart', JSON.stringify(cart));
  }

  if (cart.length === 0) { container.innerHTML = ''; if (emptyMsg) emptyMsg.style.display = 'block'; if (totalEl) totalEl.textContent = formatPrice(0); updateCartBadge(); return; }
  if (emptyMsg) emptyMsg.style.display = 'none'; container.innerHTML = '';
  let total = 0;
  cart.forEach(item => {
    const prod = products.find(p => p.id === item.id); if (!prod) return; const price = parsePrice(prod.price); const itemTotal = price * item.quantity; total += itemTotal;
    const itemDiv = document.createElement('div'); itemDiv.className = 'cart-item d-flex gap-3 mb-3 pb-3 border-bottom align-items-center'; itemDiv.setAttribute('data-product-id', prod.id);
    itemDiv.innerHTML = `
      <div class="d-flex w-100 align-items-center gap-3">
        <img src="${prod.image}" alt="${escapeHtml(prod.name)}" class="rounded cart-item-img" style="width:70px;height:70px;object-fit:cover;flex:0 0 auto;">
        <div class="flex-grow-1">
          <h6 class="mb-1 text-dark">${escapeHtml(prod.name)}</h6>
          <p class="mb-1 text-primary fw-bold">${prod.price}</p>
          <div class="d-flex align-items-center gap-2">
            <button type="button" class="btn btn-sm btn-outline-secondary" data-action="decrease" data-product-id="${prod.id}"><i class="fas fa-minus"></i></button>
            <input type="text" inputmode="numeric" pattern="[0-9]*" min="1" value="${item.quantity}" class="form-control form-control-sm text-center quantity-input" data-product-id="${prod.id}">
            <button type="button" class="btn btn-sm btn-outline-secondary" data-action="increase" data-product-id="${prod.id}"><i class="fas fa-plus"></i></button>
          </div>
        </div>
        <div class="d-flex flex-column align-items-end ms-2" style="flex:0 0 auto;">
          <div class="fw-bold cart-item-total">${formatPrice(itemTotal)}</div>
          <button type="button" class="btn btn-sm btn-outline-danger mt-2" data-action="remove" data-product-id="${prod.id}" title="Xóa"><i class="fas fa-trash"></i></button>
        </div>
      </div>`;
    container.appendChild(itemDiv);
  });
  if (totalEl) totalEl.textContent = formatPrice(total);
  updateCartBadge();
}

function removeFromCart(productId) {
  cart = cart.filter(i=>i.id!==productId);
  localStorage.setItem('cart', JSON.stringify(cart)); updateCartBadge(); renderCart();
  try { if (window.isAuthenticated) fetch('/api/cart/remove',{method:'POST',headers:{'Content-Type':'application/json'},body:JSON.stringify({productId})}); } catch(e){}
}

function updateQuantity(productId, change) {
  const item = cart.find(i=>i.id===productId); if (!item) return;
  if (change < 0 && item.quantity === 1) { if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) removeFromCart(productId); return; }
  const prod = products.find(p=>p.id===productId); const stock = prod ? (prod.stock ?? prod.StockQuantity ?? 0) : 0;
  if (change > 0 && item.quantity + change > stock) { if (window.siteEnhancements && window.siteEnhancements.showNotification) window.siteEnhancements.showNotification(`Không thể tăng: chỉ còn ${stock} trong kho`, 'warning'); return; }
  item.quantity = Math.max(0, item.quantity + change);
  if (item.quantity <= 0) { removeFromCart(productId); return; }
  localStorage.setItem('cart', JSON.stringify(cart)); renderCart();
}

function setQuantity(productId, value) {
  const qty = parseInt(value,10) || 0; const item = cart.find(i=>i.id===productId); if (!item) return; if (qty <= 0) { if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) removeFromCart(productId); return; }
  const prod = products.find(p=>p.id===productId); const stock = prod ? (prod.stock ?? prod.StockQuantity ?? 0) : 0;
  if (qty > stock) { if (window.siteEnhancements && window.siteEnhancements.showNotification) window.siteEnhancements.showNotification(`Không thể đặt ${qty}: chỉ còn ${stock} trong kho`, 'warning'); item.quantity = Math.max(1, Math.min(qty, stock)); localStorage.setItem('cart', JSON.stringify(cart)); renderCart(); return; }
  item.quantity = qty; localStorage.setItem('cart', JSON.stringify(cart)); renderCart();
}

document.addEventListener('click', (e) => {
  const btn = e.target.closest('[data-action]'); if (!btn) return; const action = btn.getAttribute('data-action'); const pid = parseInt(btn.getAttribute('data-product-id'),10); if (isNaN(pid)) return; if (action === 'decrease') updateQuantity(pid, -1); else if (action === 'increase') updateQuantity(pid, 1); else if (action === 'remove') { if (confirm('Bạn có chắc muốn xóa sản phẩm này?')) removeFromCart(pid); }
});

// Pagination renderer
function renderPagination(total, page, pageSize) {
  const container = document.getElementById('paginationContainer');
  if (!container) return;

  // If showing all items or only 1 page, hide pagination
  if (pageSize === 0 || total <= pageSize) {
    container.innerHTML = '';
    return;
  }

  const totalPages = Math.ceil(total / pageSize);
  if (totalPages <= 1) {
    container.innerHTML = '';
    return;
  }

  let html = '<nav aria-label="Product pagination"><ul class="pagination pagination-sm mb-0">';

  // Previous button
  if (page > 1) {
    html += `<li class="page-item"><a class="page-link" href="#" data-page="${page - 1}">Trước</a></li>`;
  } else {
    html += `<li class="page-item disabled"><span class="page-link">Trước</span></li>`;
  }

  // Page numbers (show max 5 pages)
  let startPage = Math.max(1, page - 2);
  let endPage = Math.min(totalPages, page + 2);

  if (page <= 3) {
    endPage = Math.min(5, totalPages);
  }
  if (page >= totalPages - 2) {
    startPage = Math.max(1, totalPages - 4);
  }

  if (startPage > 1) {
    html += `<li class="page-item"><a class="page-link" href="#" data-page="1">1</a></li>`;
    if (startPage > 2) {
      html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
    }
  }

  for (let i = startPage; i <= endPage; i++) {
    if (i === page) {
      html += `<li class="page-item active"><span class="page-link">${i}</span></li>`;
    } else {
      html += `<li class="page-item"><a class="page-link" href="#" data-page="${i}">${i}</a></li>`;
    }
  }

  if (endPage < totalPages) {
    if (endPage < totalPages - 1) {
      html += `<li class="page-item disabled"><span class="page-link">...</span></li>`;
    }
    html += `<li class="page-item"><a class="page-link" href="#" data-page="${totalPages}">${totalPages}</a></li>`;
  }

  // Next button
  if (page < totalPages) {
    html += `<li class="page-item"><a class="page-link" href="#" data-page="${page + 1}">Sau</a></li>`;
  } else {
    html += `<li class="page-item disabled"><span class="page-link">Sau</span></li>`;
  }

  html += '</ul></nav>';
  container.innerHTML = html;

  // Add click handlers for pagination links
  container.querySelectorAll('a[data-page]').forEach(link => {
    link.addEventListener('click', (e) => {
      e.preventDefault();
      const newPage = parseInt(link.getAttribute('data-page'), 10);
      if (newPage && newPage !== page) {
        loadProducts(newPage, currentPageSize, currentCategory, currentSort, currentSearch);
        window.scrollTo({ top: 0, behavior: 'smooth' });
      }
    });
  });
}

// Init
document.addEventListener('DOMContentLoaded', ()=>{
  const initialCategory = (new URLSearchParams(window.location.search)).get('category') || 'all';
  loadProducts(1, currentPageSize, initialCategory);

  // Event listeners for controls
  const sortSelect = document.getElementById('sortSelect');
  const pageSizeSelect = document.getElementById('pageSizeSelect');

  if (sortSelect) {
    sortSelect.addEventListener('change', (e) => {
      currentSort = e.target.value;
      loadProducts(1, currentPageSize, currentCategory, currentSort, currentSearch);
    });
  }

  if (pageSizeSelect) {
    pageSizeSelect.addEventListener('change', (e) => {
      const newSize = parseInt(e.target.value, 10) || 0;
      currentPageSize = newSize;
      loadProducts(1, newSize, currentCategory, currentSort, currentSearch);
    });
  }

  // Restore cart from server if authenticated
  if (window.isAuthenticated) (async ()=>{ try { const resp = await fetch('/api/cart/load'); if (resp.ok) { const json = await resp.json(); if (json && json.cartJson) { const serverCart = JSON.parse(json.cartJson || '[]'); const local = JSON.parse(localStorage.getItem('cart')||'[]'); if ((!local || local.length === 0) && serverCart && serverCart.length>0) { localStorage.setItem('cart', JSON.stringify(serverCart)); cart = serverCart; updateCartBadge(); renderCart(); } } } }catch(e){} })();
});

// Expose functions to global scope
window.showProductDetailModal = showProductDetailModal;
window.addToCartFromModal = addToCartFromModal;
