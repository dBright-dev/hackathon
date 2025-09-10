// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function toggleFab() {
    const fabContainer = document.getElementById('fabContainer');
    fabContainer.classList.toggle('active');
}

function showFaq() {
    // Scroll to FAQ section on contact page
    window.location.href = '@Url.Action("Contact", "Home")#faqAccordion';
}

// Close FAB when clicking outside
document.addEventListener('click', function (event) {
    const fabContainer = document.getElementById('fabContainer');
    if (!fabContainer.contains(event.target) && fabContainer.classList.contains('active')) {
        fabContainer.classList.remove('active');
    }
});

// Close FAB on escape key
document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape') {
        const fabContainer = document.getElementById('fabContainer');
        fabContainer.classList.remove('active');
    }
});