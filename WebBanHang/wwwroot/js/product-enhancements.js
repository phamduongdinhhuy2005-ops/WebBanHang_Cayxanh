/**
 * product-enhancements.js
 * Enhanced interactions for product page
 */

document.addEventListener('DOMContentLoaded', () => {
    // Quick Filter Functionality
    const filterButtons = document.querySelectorAll('.quick-filters .btn[data-filter]');
    
    filterButtons.forEach(btn => {
        btn.addEventListener('click', function() {
            // Remove active from all buttons
            filterButtons.forEach(b => b.classList.remove('active'));
            
            // Add active to clicked button
            this.classList.add('active');
            
            const filter = this.getAttribute('data-filter');
            applyQuickFilter(filter);
        });
    });
    
    // Apply quick filter
    function applyQuickFilter(filter) {
        switch(filter) {
            case 'all':
                loadProducts(1, currentPageSize, 'all', currentSort, '');
                break;
            case 'sale':
                loadProducts(1, currentPageSize, 'khuyenmai', currentSort, '');
                break;
            case 'laptop':
                loadProducts(1, currentPageSize, 'laptop', currentSort, '');
                break;
            case 'phukien':
                loadProducts(1, currentPageSize, 'phukien', currentSort, '');
                break;
            case 'new':
                loadProducts(1, currentPageSize, currentCategory, 'newest', '');
                break;
            case 'lowstock':
                // Filter products with stock < 10 on frontend
                filterLowStock();
                break;
        }
    }
    
    // Filter low stock products (client-side)
    function filterLowStock() {
        const allProducts = [...products];
        const lowStockProducts = allProducts.filter(p => {
            const stock = p.stock ?? p.StockQuantity ?? 0;
            return stock > 0 && stock < 10;
        });
        
        // Temporarily replace products array
        const originalProducts = [...products];
        products = lowStockProducts;
        renderProducts();
        
        // Update count
        const countEl = document.getElementById('productsCount');
        if (countEl) {
            countEl.textContent = `${lowStockProducts.length} sản phẩm`;
        }
        
        // Hide pagination
        const paginationContainer = document.getElementById('paginationContainer');
        if (paginationContainer) {
            paginationContainer.innerHTML = '';
        }
    }
    
    // Smooth scroll to deals section
    const dealsLink = document.querySelector('a[href="#deals"]');
    if (dealsLink) {
        dealsLink.addEventListener('click', function(e) {
            e.preventDefault();
            const productsSection = document.querySelector('#productsContainer');
            if (productsSection) {
                productsSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
                
                // Flash highlight on first discount product
                setTimeout(() => {
                    const firstDeal = document.querySelector('.card .badge.bg-danger');
                    if (firstDeal) {
                        const card = firstDeal.closest('.product-card-hover');
                        if (card) {
                            card.style.animation = 'pulse 1s ease 2';
                        }
                    }
                }, 500);
            }
        });
    }
    
    // Newsletter form handler (placeholder)
    const newsletterForm = document.querySelector('.input-group');
    if (newsletterForm) {
        const btn = newsletterForm.querySelector('.btn');
        const input = newsletterForm.querySelector('input[type="email"]');
        
        if (btn && input) {
            btn.addEventListener('click', function() {
                const email = input.value.trim();
                if (email && email.includes('@')) {
                    // Show success message
                    if (window.siteEnhancements && window.siteEnhancements.showNotification) {
                        window.siteEnhancements.showNotification('Cảm ơn bạn đã đăng ký nhận tin!', 'success');
                    } else {
                        alert('Cảm ơn bạn đã đăng ký nhận tin!');
                    }
                    input.value = '';
                } else {
                    if (window.siteEnhancements && window.siteEnhancements.showNotification) {
                        window.siteEnhancements.showNotification('Vui lòng nhập email hợp lệ', 'warning');
                    } else {
                        alert('Vui lòng nhập email hợp lệ');
                    }
                }
            });
            
            // Enter key support
            input.addEventListener('keypress', function(e) {
                if (e.key === 'Enter') {
                    btn.click();
                }
            });
        }
    }
    
    // Add pulse animation for deals
    const style = document.createElement('style');
    style.textContent = `
        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); box-shadow: 0 16px 32px rgba(102, 126, 234, 0.3); }
        }
    `;
    document.head.appendChild(style);
    
    // Lazy load optimization for images
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    if (img.dataset.src) {
                        img.src = img.dataset.src;
                        img.removeAttribute('data-src');
                        observer.unobserve(img);
                    }
                }
            });
        });
        
        // Observe images when they're rendered
        const observer = new MutationObserver(() => {
            document.querySelectorAll('img[data-src]').forEach(img => {
                imageObserver.observe(img);
            });
        });
        
        observer.observe(document.getElementById('productsContainer'), {
            childList: true,
            subtree: true
        });
    }
    
    // Add "Back to Top" button
    const backToTop = document.createElement('button');
    backToTop.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTop.className = 'btn btn-primary rounded-circle position-fixed bottom-0 end-0 m-4 shadow-lg';
    backToTop.style.cssText = 'width: 50px; height: 50px; opacity: 0; transition: opacity 0.3s; z-index: 1000; display: none;';
    document.body.appendChild(backToTop);
    
    window.addEventListener('scroll', () => {
        if (window.scrollY > 300) {
            backToTop.style.display = 'block';
            setTimeout(() => backToTop.style.opacity = '1', 10);
        } else {
            backToTop.style.opacity = '0';
            setTimeout(() => backToTop.style.display = 'none', 300);
        }
    });
    
    backToTop.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
});
