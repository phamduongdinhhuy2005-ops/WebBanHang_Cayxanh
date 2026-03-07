/**
 * ==============================================
 * ENHANCEMENTS.JS
 * Các tính n?ng nâng cao cho website
 * ==============================================
 */

// ========== NÚT QUAY L?I ??U TRANG ==========
/**
 * T?o nút "Back to Top" ?? scroll lên ??u trang
 */
function createBackToTopButton() {
    const backToTopBtn = document.createElement('button');
    backToTopBtn.innerHTML = '<i class="fas fa-arrow-up"></i>';
    backToTopBtn.className = 'btn btn-primary back-to-top';
    backToTopBtn.setAttribute('aria-label', 'Cu?n lên ??u trang');
    backToTopBtn.style.cssText = 'position: fixed; bottom: 30px; right: 30px; width: 50px; height: 50px; border-radius: 50%; display: none; z-index: 1000; box-shadow: 0 4px 12px rgba(0,0,0,0.2); transition: all 0.3s ease;';
    document.body.appendChild(backToTopBtn);
    
    // Hi?n th? nút khi scroll xu?ng > 300px
    window.addEventListener('scroll', () => {
        if (window.pageYOffset > 300) {
            backToTopBtn.style.display = 'block';
            backToTopBtn.style.opacity = '1';
        } else {
            backToTopBtn.style.opacity = '0';
            setTimeout(() => {
                if (window.pageYOffset <= 300) {
                    backToTopBtn.style.display = 'none';
                }
            }, 300);
        }
    });
    
    // X? lý click - cu?n m??t v? ??u trang
    backToTopBtn.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
    });
    
    // Hi?u ?ng hover
    backToTopBtn.addEventListener('mouseenter', () => {
        backToTopBtn.style.transform = 'scale(1.1)';
    });
    
    backToTopBtn.addEventListener('mouseleave', () => {
        backToTopBtn.style.transform = 'scale(1)';
    });
}

// ========== X? LÝ TÌM KI?M T? URL ==========
/**
 * X? lý tham s? search t? URL (ví d?: ?search=laptop)
 * T? ??ng ?i?n vào ô tìm ki?m và th?c hi?n tìm ki?m
 */
function handleSearchFromURL() {
    const urlParams = new URLSearchParams(window.location.search);
    const searchParam = urlParams.get('search');
    if (searchParam) {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.value = searchParam;
            searchInput.dispatchEvent(new Event('input'));
            
            // Cu?n t?i ph?n s?n ph?m sau 300ms
            setTimeout(() => {
                const productsSection = document.getElementById('products');
                if (productsSection) {
                    productsSection.scrollIntoView({ behavior: 'smooth' });
                }
            }, 300);
        }
    }
}

// ========== LAZY LOADING HÌNH ?NH ==========
/**
 * Thi?t l?p lazy loading cho hình ?nh
 * Ch? t?i hình khi xu?t hi?n trong viewport (ti?t ki?m bandwidth)
 */
function setupImageLoading() {
    const images = document.querySelectorAll('img[data-src]');
    
    // S? d?ng IntersectionObserver n?u trình duy?t h? tr?
    if ('IntersectionObserver' in window) {
        const imageObserver = new IntersectionObserver((entries, observer) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.removeAttribute('data-src');
                    observer.unobserve(img);
                }
            });
        });
        
        images.forEach(img => imageObserver.observe(img));
    } else {
        // Fallback cho trình duy?t c? - t?i t?t c? ngay
        images.forEach(img => {
            img.src = img.dataset.src;
            img.removeAttribute('data-src');
        });
    }
}

// ========== HI?U ?NG PULSE CHO ICON GI? HÀNG ==========
/**
 * Thêm hi?u ?ng pulse cho icon gi? hàng khi có thay ??i
 */
function pulseCartIcon() {
    const cartIcon = document.querySelector('.fa-shopping-cart');
    if (cartIcon) {
        cartIcon.classList.add('pulse');
        setTimeout(() => cartIcon.classList.remove('pulse'), 500);
    }
}

// ========== HI?N TH? THÔNG BÁO TOAST ==========
/**
 * Hi?n th? thông báo d?ng toast
 * @param {string} message - N?i dung thông báo
 * @param {string} type - Lo?i thông báo: success, danger, warning, info
 */
function showNotification(message, type = 'success') {
    const toastContainer = document.querySelector('.toast');
    if (toastContainer) {
        const toastBody = toastContainer.querySelector('.toast-body');
        if (toastBody) {
            toastBody.textContent = message;
            
            // Thay ??i màu toast d?a trên lo?i
            toastContainer.className = `toast text-bg-${type}`;
            
            // Hi?n th? toast
            const toast = new bootstrap.Toast(toastContainer);
            toast.show();
        }
    }
}

// ========== SMOOTH SCROLL CHO ANCHOR LINKS ==========
/**
 * Thi?t l?p smooth scroll cho các link anchor (#section)
 */
function setupSmoothScroll() {
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            const href = this.getAttribute('href');
            // Ch? x? lý n?u không ph?i # ??n thu?n
            if (href !== '#' && href.length > 1) {
                const target = document.querySelector(href);
                if (target) {
                    e.preventDefault();
                    target.scrollIntoView({
                        behavior: 'smooth',
                        block: 'start'
                    });
                }
            }
        });
    });
}

// ========== PHÍM T?T BÀN PHÍM ==========
/**
 * Thi?t l?p các phím t?t ?? c?i thi?n accessibility
 * - ESC: ?óng modal và offcanvas
 */
function setupKeyboardNav() {
    document.addEventListener('keydown', (e) => {
        // Phím ESC ?óng modal và offcanvas
        if (e.key === 'Escape') {
            // ?óng modal n?u ?ang m?
            const openModal = document.querySelector('.modal.show');
            if (openModal) {
                const modalInstance = bootstrap.Modal.getInstance(openModal);
                if (modalInstance) modalInstance.hide();
            }
            
            // ?óng offcanvas n?u ?ang m?
            const openOffcanvas = document.querySelector('.offcanvas.show');
            if (openOffcanvas) {
                const offcanvasInstance = bootstrap.Offcanvas.getInstance(openOffcanvas);
                if (offcanvasInstance) offcanvasInstance.hide();
            }
        }
    });
}

// ========== KH?I T?O T?T C? CÁC TÍNH N?NG ==========
/**
 * Hàm kh?i t?o t?t c? các enhancement
 * G?i hàm này khi DOM ?ã s?n sàng
 */
function initEnhancements() {
    createBackToTopButton();
    handleSearchFromURL();
    setupImageLoading();
    setupSmoothScroll();
    setupKeyboardNav();
}

// ========== EXPORT FUNCTIONS ==========
// Export các functions ?? s? d?ng trong file JavaScript khác
if (typeof window !== 'undefined') {
    window.siteEnhancements = {
        createBackToTopButton,
        handleSearchFromURL,
        pulseCartIcon,
        showNotification,
        setupSmoothScroll,
        setupKeyboardNav,
        initEnhancements
    };
}


