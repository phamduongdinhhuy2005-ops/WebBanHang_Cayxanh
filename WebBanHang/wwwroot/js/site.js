// ==============================================
// SITE.JS - JavaScript chung cho toàn website
// ==============================================

// Global notification system
window.siteEnhancements = window.siteEnhancements || {};
window.siteEnhancements.showNotification = function(message, type = 'info') {
    type = type || 'info';
    const typeMap = {
        'success': { icon: 'fa-check-circle', bg: 'bg-success' },
        'error': { icon: 'fa-exclamation-circle', bg: 'bg-danger' },
        'warning': { icon: 'fa-exclamation-triangle', bg: 'bg-warning' },
        'info': { icon: 'fa-info-circle', bg: 'bg-info' }
    };
    const config = typeMap[type] || typeMap['info'];

    // Create notification container if not exists
    let container = document.getElementById('notificationContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'notificationContainer';
        container.style.cssText = 'position:fixed;top:80px;right:20px;z-index:9999;min-width:300px;max-width:400px;';
        document.body.appendChild(container);
    }

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `alert ${config.bg} text-white alert-dismissible fade show shadow-lg mb-2`;
    notification.style.cssText = 'animation: slideInRight 0.3s ease;';
    notification.innerHTML = `
        <i class="fas ${config.icon} me-2"></i>
        <span>${message}</span>
        <button type="button" class="btn-close btn-close-white" data-bs-dismiss="alert"></button>
    `;

    container.appendChild(notification);

    // Auto remove after 4 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => notification.remove(), 300);
    }, 4000);

    // Add animation keyframes if not exists
    if (!document.getElementById('notificationStyles')) {
        const style = document.createElement('style');
        style.id = 'notificationStyles';
        style.textContent = `
            @keyframes slideInRight {
                from { transform: translateX(100%); opacity: 0; }
                to { transform: translateX(0); opacity: 1; }
            }
        `;
        document.head.appendChild(style);
    }
};

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

