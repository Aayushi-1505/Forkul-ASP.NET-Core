(() => {
  'use strict';
  const toast = document.getElementById('toast-message');
  let toastTimer;
  function showToast(message) {
    if (!toast || !message) return;
    toast.textContent = `✓  ${message}`;
    toast.classList.add('show');
    clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('show'), 3600);
  }
  if (toast?.dataset.message) showToast(toast.dataset.message);

  const menuButton = document.querySelector('.mobile-menu-toggle');
  const nav = document.getElementById('main-nav');
  menuButton?.addEventListener('click', () => {
    const open = nav.classList.toggle('open');
    menuButton.setAttribute('aria-expanded', String(open));
    menuButton.setAttribute('aria-label', open ? 'Close menu' : 'Open menu');
  });
  nav?.querySelectorAll('a').forEach(link => link.addEventListener('click', () => {
    nav.classList.remove('open');
    menuButton?.setAttribute('aria-expanded', 'false');
  }));

  // Progressive enhancement: the forms still work normally with JavaScript disabled.
  document.querySelectorAll('.add-to-cart-form').forEach(form => {
    form.addEventListener('submit', async event => {
      event.preventDefault();
      const button = form.querySelector('[type="submit"]');
      button.disabled = true;
      try {
        const response = await fetch(form.action, {
          method: 'POST', body: new FormData(form), credentials: 'same-origin',
          headers: { 'X-Requested-With': 'XMLHttpRequest', 'Accept': 'application/json' }
        });
        if (!response.ok) throw new Error('Could not add this item. Please try again.');
        const result = await response.json();
        document.querySelectorAll('[data-cart-count]').forEach(count => { count.textContent = result.count; });
        showToast(result.message);
      } catch (error) {
        showToast(error.message || 'Something went wrong. Please try again.');
      } finally { button.disabled = false; }
    });
  });

  // Favorites are local to this browser and do not need a login or a server request.
  let favorites = [];
  try { favorites = JSON.parse(localStorage.getItem('forkful-favorites') || '[]'); } catch { favorites = []; }
  document.querySelectorAll('[data-favorite-id]').forEach(button => {
    const id = Number(button.dataset.favoriteId);
    button.setAttribute('aria-pressed', String(favorites.includes(id)));
    button.addEventListener('click', () => {
      favorites = favorites.includes(id) ? favorites.filter(x => x !== id) : [...favorites, id];
      button.setAttribute('aria-pressed', String(favorites.includes(id)));
      try { localStorage.setItem('forkful-favorites', JSON.stringify(favorites)); } catch { /* storage optional */ }
      showToast(favorites.includes(id) ? 'Saved to favorites on this device' : 'Removed from favorites');
    });
  });

  document.querySelectorAll('[data-auto-submit]').forEach(select => {
    select.addEventListener('change', () => select.form?.requestSubmit());
  });

  document.querySelectorAll('.quantity-picker').forEach(picker => {
    const input = picker.querySelector('input[type="number"]');
    if (!input) return;
    picker.querySelector('[data-qty-minus]')?.addEventListener('click', () => {
      input.value = Math.max(1, Number(input.value || 1) - 1);
      input.dispatchEvent(new Event('input', { bubbles: true }));
    });
    picker.querySelector('[data-qty-plus]')?.addEventListener('click', () => {
      input.value = Math.min(20, Number(input.value || 1) + 1);
      input.dispatchEvent(new Event('input', { bubbles: true }));
    });
  });

  const customizeForm = document.querySelector('[data-customization-form]');
  if (customizeForm) {
    const total = customizeForm.querySelector('[data-custom-total]');
    const quantity = customizeForm.querySelector('input[name="quantity"]');
    const extras = customizeForm.querySelectorAll('[data-addon-price]');
    const formatPrice = value => '₹' + new Intl.NumberFormat('en-IN', { maximumFractionDigits: 2 }).format(value);
    function updatePrice() {
      const base = Number(customizeForm.dataset.basePrice || 0);
      const addOns = [...extras].reduce((sum, input) => sum + (input.checked ? Number(input.dataset.addonPrice || 0) : 0), 0);
      const count = Math.max(1, Math.min(20, Number(quantity?.value || 1)));
      total.textContent = formatPrice((base + addOns) * count);
    }
    extras.forEach(input => input.addEventListener('change', updatePrice));
    quantity?.addEventListener('input', updatePrice);
    updatePrice();
  }

  const deliveryChoices = document.querySelectorAll('input[name="DeliveryType"]');
  const addressField = document.getElementById('address-field');
  const paymentInput = document.querySelector('input[name="PaymentMethod"]');
  const paymentLabel = document.getElementById('payment-label');
  const feeLabel = document.getElementById('checkout-delivery-fee');
  const totalLabel = document.getElementById('checkout-total');
  function syncDelivery() {
    const isPickup = document.querySelector('input[name="DeliveryType"]:checked')?.value === 'Pickup';
    if (addressField) addressField.hidden = isPickup;
    if (paymentInput) paymentInput.value = isPickup ? 'Pay at pickup' : 'Cash on delivery';
    if (paymentLabel) paymentLabel.textContent = isPickup ? 'Pay at pickup' : 'Cash on delivery';
    if (feeLabel && totalLabel) {
      const fee = isPickup ? 0 : Number(feeLabel.dataset.deliveryFee || 0);
      feeLabel.textContent = fee === 0 ? 'FREE' : `₹${fee}`;
      totalLabel.textContent = `₹${Number(totalLabel.dataset.subtotal || 0) + fee}`;
    }
  }
  deliveryChoices.forEach(radio => radio.addEventListener('change', syncDelivery));
  if (deliveryChoices.length) syncDelivery();
})();
