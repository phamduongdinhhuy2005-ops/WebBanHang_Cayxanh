/**
 * product-api.js
 * Module nh? ?? tách vi?c g?i API s?n ph?m và cache k?t qu? ? client.
 * M?c ?ích: tránh fetch /api/products nhi?u l?n (Product page và Checkout s? reuse).
 */
(function (global) {
    // simple in-memory cache on window for demo
    const cache = {
        products: null,
        timestamp: 0
    };

    // Th?i gian cache (ms)
    const CACHE_TTL = 60 * 1000; // 1 phút

    async function fetchProducts(params) {
        // params: object -> query string
        const qs = new URLSearchParams();
        if (params) {
            Object.keys(params).forEach(k => {
                if (params[k] !== undefined && params[k] !== null && params[k] !== '') qs.set(k, params[k]);
            });
        }
        const url = '/api/products' + (qs.toString() ? ('?' + qs.toString()) : '');

        // If no params and cache is fresh, return cached
        if ((!params || Object.keys(params).length === 0) && cache.products && (Date.now() - cache.timestamp) < CACHE_TTL) {
            return cache.products;
        }

        const resp = await fetch(url);
        if (!resp.ok) throw new Error('Failed to fetch products');
        const data = await resp.json();

        // store in cache only when fetching full list (no params) or when data.items exists
        if ((!params || Object.keys(params).length === 0) || data.items) {
            cache.products = data;
            cache.timestamp = Date.now();
        }
        return data;
    }

    // Convenience: get full product list once (caches result)
    async function getProductsOnce() {
        const data = await fetchProducts();
        // API may return { items, total } or plain array
        return data.items || data;
    }

    global.productApi = {
        fetchProducts,
        getProductsOnce
    };
})(window);
