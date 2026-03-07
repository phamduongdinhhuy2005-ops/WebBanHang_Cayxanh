// ==============================================
// SITE.JS - JavaScript chung cho toàn website
// ==============================================

// ========== SMOOTH SCROLL TO CONTACT + NAV SEARCH ==========
document.addEventListener('DOMContentLoaded', () => {
    // Clicking contact links in nav should navigate to /Home/Contact
    const contactLinks = document.querySelectorAll('.scroll-to-contact');
    contactLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            window.location.href = '/Home/Contact';
        });
    });

    // Global navbar search behavior: if user submits from non-Product pages, redirect to Product with query
    const navForm = document.getElementById('navSearchForm');
    const navInput = document.getElementById('navSearchInput');
    if (navForm && navInput) {
        navForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const term = navInput.value?.trim();
            if (!term) return;

            if (!window.location.pathname.toLowerCase().startsWith('/product')) {
                // redirect to product page with search
                window.location.href = '/Product?search=' + encodeURIComponent(term);
                return;
            }

            // If on Product page, use product page handler
            if (window.handleProductNavSearch) {
                window.handleProductNavSearch(term);
            } else {
                // fallback: navigate to same page with query
                window.location.href = '/Product?search=' + encodeURIComponent(term);
            }
        });

// Khi trang sản phẩm tải, khởi tạo controls và tải products
document.addEventListener('DOMContentLoaded', () => {
    if (window.setupProductControls) window.setupProductControls();
    if (window.setupSearch) window.setupSearch();
    // Nếu ở trang Product, gọi loadProducts với state mặc định
    if (window.location.pathname.toLowerCase().startsWith('/product') && window.loadProducts) {
        // lấy page/pageSize từ UI nếu có
        const psEl = document.getElementById('pageSizeSelect');
        const catEl = document.getElementById('categoryFilter');
        const sortEl = document.getElementById('sortSelect');
        const pageSize = psEl ? Number(psEl.value) || 9 : 9;
        const category = catEl ? catEl.value || 'all' : 'all';
        const sort = sortEl ? sortEl.value || '' : '';
        window.loadProducts(1, pageSize, category, sort, new URLSearchParams(window.location.search).get('search') || '');
    }
    // Mini-cart hover preview
    const cartBtn = document.getElementById('cartButton');
    const miniPreview = document.getElementById('miniCartPreview');
    if (cartBtn && miniPreview) {
        cartBtn.addEventListener('mouseenter', () => {
            // render mini cart and show
            if (window.renderMiniCart) window.renderMiniCart();
            miniPreview.style.display = 'block';
        });
        cartBtn.addEventListener('mouseleave', () => {
            // hide after short delay to allow hover inside
            setTimeout(() => {
                if (!miniPreview.matches(':hover')) miniPreview.style.display = 'none';
            }, 250);
        });
        miniPreview.addEventListener('mouseleave', () => miniPreview.style.display = 'none');
        miniPreview.addEventListener('mouseenter', () => miniPreview.style.display = 'block');
    }
});
    }
});



