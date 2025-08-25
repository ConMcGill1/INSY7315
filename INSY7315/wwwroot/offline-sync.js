
const DB_NAME = 'insy7315-offline';
const STORE = 'outbox';

function idb() {
    return new Promise((res, rej) => {
        const open = indexedDB.open(DB_NAME, 1);
        open.onupgradeneeded = () => open.result.createObjectStore(STORE, { keyPath: 'k' });
        open.onsuccess = () => res(open.result);
        open.onerror = () => rej(open.error);
    });
}

async function putOutbox(item) {
    const db = await idb(); const tx = db.transaction(STORE, 'readwrite');
    tx.objectStore(STORE).put(item); return tx.complete;
}
async function allOutbox() {
    const db = await idb(); const tx = db.transaction(STORE, 'readonly');
    return await tx.objectStore(STORE).getAll();
}
async function delOutbox(k) {
    const db = await idb(); const tx = db.transaction(STORE, 'readwrite');
    tx.objectStore(STORE).delete(k); return tx.complete;
}

async function drainOutbox() {
    const items = await allOutbox();
    for (const it of items) {
        try {
            const r = await fetch(it.url, { method: it.method, headers: it.headers, body: it.body });
            if (r.ok) await delOutbox(it.k);
        } catch { /* still offline or server unreachable */ }
    }
}


function formToDto(form) {
    return {
        Id: form.elements['Id']?.value ? parseInt(form.elements['Id'].value) : null,
        Name: form.elements['Product.Name']?.value || form.elements['Name']?.value || "",
        Price: parseFloat(form.elements['Product.Price']?.value || form.elements['Price']?.value || "0"),
        Owner: form.elements['Product.Owner']?.value || form.elements['Owner']?.value || "",
        Model: form.elements['Product.Model']?.value || form.elements['Model']?.value || null,
        Category: form.elements['Product.Category']?.value || form.elements['Category']?.value || null
    };
}


async function submitProductForm(e, mode, id) {
    e.preventDefault();
    const dto = formToDto(e.target);
    const url = mode === 'create' ? '/api/products'
        : mode === 'edit' ? `/api/products/${id}`
            : `/api/products/${id}`;
    const method = mode === 'create' ? 'POST' : (mode === 'edit' ? 'PUT' : 'DELETE');
    const headers = { 'Content-Type': 'application/json' };
    const body = method === 'DELETE' ? null : JSON.stringify(dto);

    if (navigator.onLine) {
        const r = await fetch(url, { method, headers, body });
        if (!r.ok) { alert('Save failed.'); return false; }
    } else {
        await putOutbox({ k: Date.now() + Math.random(), url, method, headers, body });
        alert('Saved offline. Will sync when you are back online.');
    }

    
    window.location.href = '/';
    return false;
}

window.addEventListener('online', drainOutbox);
document.addEventListener('DOMContentLoaded', () => { if (navigator.onLine) drainOutbox(); });

window.__insy7315_sync = { submitProductForm, drainOutbox }; 
