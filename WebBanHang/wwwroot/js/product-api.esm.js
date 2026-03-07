// ES module: product-api.esm.js
// Export functions to call /api/products. This is modern module version.

const CACHE_TTL = 60 * 1000; // 1 minute
let cache = { products: null, timestamp: 0 };

export async function fetchProducts(params = {}) {
  const qs = new URLSearchParams();
  Object.keys(params || {}).forEach(k => {
    const v = params[k];
    if (v !== undefined && v !== null && v !== '') qs.set(k, v);
  });
  const url = '/api/products' + (qs.toString() ? ('?' + qs.toString()) : '');

  if ((!params || Object.keys(params).length === 0) && cache.products && (Date.now() - cache.timestamp) < CACHE_TTL) {
    return cache.products;
  }

  const resp = await fetch(url);
  if (!resp.ok) throw new Error('Failed to fetch products');
  const data = await resp.json();

  if ((!params || Object.keys(params).length === 0) || data.items) {
    cache.products = data;
    cache.timestamp = Date.now();
  }
  return data;
}

export async function getProductsOnce() {
  const data = await fetchProducts();
  return data.items || data;
}
