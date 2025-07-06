// Google Login Handler
document.addEventListener('DOMContentLoaded', function() {
    // Add click event for Google login button
    const googleLoginBtn = document.getElementById('google-login-btn');
    if (googleLoginBtn) {
        googleLoginBtn.addEventListener('click', handleGoogleLogin);
    }
    
    // Check if we have an error in URL (from backend callback)
    const urlParams = new URLSearchParams(window.location.search);
    const error = urlParams.get('error');
    
    if (error) {
        showError('Google authentication failed: ' + error);
    }
});

async function handleGoogleLogin() {
    try {
        // Show loading state
        const googleBtn = document.getElementById('google-login-btn');
        const originalText = googleBtn.innerHTML;
        googleBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>Loading...';
        googleBtn.disabled = true;
        
        // Call API to get Google OAuth URL
        const response = await fetch('/api/GoogleAuth/login-url', {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Accept': 'application/json'
            }
        });
        
        if (!response.ok) {
            throw new Error('Failed to get Google login URL');
        }
        
        const data = await response.json();
        
        if (data.url) {
            // Redirect to Google OAuth URL
            window.location.href = data.url;
        } else {
            throw new Error('No URL received from API');
        }
        
    } catch (error) {
        console.error('Google login error:', error);
        showError('Failed to initiate Google login. Please try again.');
        
        // Reset button state
        const googleBtn = document.getElementById('google-login-btn');
        googleBtn.innerHTML = '<i class="fab fa-google me-2"></i>Google';
        googleBtn.disabled = false;
    }
}



// Utility functions
function setCookie(name, value, days) {
    const expires = new Date();
    expires.setTime(expires.getTime() + (days * 24 * 60 * 60 * 1000));
    document.cookie = `${name}=${value};expires=${expires.toUTCString()};path=/;SameSite=Lax`;
}

function getCookie(name) {
    const nameEQ = name + "=";
    const ca = document.cookie.split(';');
    for(let i = 0; i < ca.length; i++) {
        let c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) === 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}

function showError(message) {
    const errorDiv = document.getElementById('error-message');
    if (errorDiv) {
        errorDiv.innerText = message;
        errorDiv.style.display = 'block';
    }
}

function showMessage(message, type = 'info') {
    // Create a temporary message element
    const messageDiv = document.createElement('div');
    messageDiv.className = `alert alert-${type === 'success' ? 'success' : 'info'} alert-dismissible fade show`;
    messageDiv.innerHTML = `
        ${message}
        <button type="button" class="btn-close" data-bs-dismiss="alert"></button>
    `;
    
    // Insert at the top of the form
    const form = document.getElementById('login-form');
    if (form) {
        form.parentNode.insertBefore(messageDiv, form);
    }
    
    // Auto-remove after 5 seconds
    setTimeout(() => {
        if (messageDiv.parentNode) {
            messageDiv.remove();
        }
    }, 5000);
} 