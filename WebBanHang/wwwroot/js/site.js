// ==============================================
// SITE.JS - JavaScript chung cho toàn website
// ==============================================

document.addEventListener('DOMContentLoaded', () => {
    // ========== CONTACT LINKS ==========
    const contactLinks = document.querySelectorAll('.scroll-to-contact');
    contactLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            e.preventDefault();
            window.location.href = '/Home/Contact';
        });
    });

    // ========== NAVBAR SEARCH ==========
    const navForm = document.getElementById('navSearchForm');
    const navInput = document.getElementById('navSearchInput');
    if (navForm && navInput) {
        navForm.addEventListener('submit', function (e) {
            e.preventDefault();
            const term = navInput.value?.trim();
            if (!term) return;

            if (!window.location.pathname.toLowerCase().startsWith('/product')) {
                window.location.href = '/Product?search=' + encodeURIComponent(term);
                return;
            }

            if (window.handleProductNavSearch) {
                window.handleProductNavSearch(term);
            } else {
                window.location.href = '/Product?search=' + encodeURIComponent(term);
            }
        });
    }

    // ========== PRODUCT PAGE INITIALIZATION ==========
    if (window.location.pathname.toLowerCase().startsWith('/product')) {
        if (window.setupProductControls) window.setupProductControls();
        if (window.setupSearch) window.setupSearch();

        if (window.loadProducts) {
            const psEl = document.getElementById('pageSizeSelect');
            const catEl = document.getElementById('categoryFilter');
            const sortEl = document.getElementById('sortSelect');
            const pageSize = psEl ? Number(psEl.value) || 8 : 8;
            const category = catEl ? catEl.value || 'all' : 'all';
            const sort = sortEl ? sortEl.value || '' : '';
            const search = new URLSearchParams(window.location.search).get('search') || '';
            window.loadProducts(1, pageSize, category, sort, search);
        }
    }

    // ========== MINI CART HOVER PREVIEW ==========
    const cartBtn = document.getElementById('cartButton');
    const miniPreview = document.getElementById('miniCartPreview');
    if (cartBtn && miniPreview) {
        cartBtn.addEventListener('mouseenter', () => {
            if (window.renderMiniCart) window.renderMiniCart();
            miniPreview.style.display = 'block';
        });

        cartBtn.addEventListener('mouseleave', () => {
            setTimeout(() => {
                if (!miniPreview.matches(':hover')) {
                    miniPreview.style.display = 'none';
                }
            }, 250);
        });

        miniPreview.addEventListener('mouseleave', () => {
            miniPreview.style.display = 'none';
        });

        miniPreview.addEventListener('mouseenter', () => {
            miniPreview.style.display = 'block';
        });
    }
});

