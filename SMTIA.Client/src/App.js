import React, { useState, useEffect } from 'react';
import './App.css';
import LoadingPage from './pages/LoadingPage/LoadingPage';
import IntroPage from './pages/IntroPage/IntroPage';
import DashboardPage from './pages/DashboardPage/DashboardPage';
import EmailConfirmationPage from './pages/EmailConfirmationPage/EmailConfirmationPage';
import ResetPasswordPage from './pages/ResetPasswordPage/ResetPasswordPage';
import { clearToken, getToken, getUserFromToken, isTokenExpired, setToken } from './services/auth';

function App() {
  const [isLoading, setIsLoading] = useState(false);
  const [showIntro, setShowIntro] = useState(true);
  const [showMainApp, setShowMainApp] = useState(false);
  const [authUser, setAuthUser] = useState(null);
  const [currentPage, setCurrentPage] = useState(null);

  useEffect(() => {
    // Check URL path for special pages
    const path = window.location.pathname;
    
    if (path === '/email-confirmation' || path.startsWith('/email-confirmation')) {
      setCurrentPage('email-confirmation');
      return;
    }
    
    if (path === '/reset-password' || path.startsWith('/reset-password')) {
      setCurrentPage('reset-password');
      return;
    }
    
    // Check for token in URL (from email confirmation or password reset)
    const urlParams = new URLSearchParams(window.location.search);
    const tokenFromUrl = urlParams.get('token');
    if (tokenFromUrl) {
      console.log('[App] Token found in URL, saving to localStorage');
      setToken(tokenFromUrl);
      const user = getUserFromToken(tokenFromUrl);
      if (user) {
        setAuthUser(user);
        setShowIntro(false);
        setShowMainApp(true);
        // Clean URL
        window.history.replaceState({}, document.title, window.location.pathname);
        return;
      }
    }
   
    setIsLoading(true);
    setShowIntro(false);
  }, []);

  const handleLoadingComplete = () => {
    setIsLoading(false);
    // If we already have a token, go directly to dashboard
    const token = getToken(); // getToken() now validates and clears expired tokens
    console.log('[App] Token kontrolü:', token ? `Token var (uzunluk: ${token.length})` : 'Token yok');
    
    if (token) {
      // Token is valid (getToken already checked expiration)
      const user = getUserFromToken(token);
      if (user && user.id) {
        console.log('[App] Token geçerli, kullanıcı:', user);
        setAuthUser(user);
        setShowIntro(false);
        setShowMainApp(true);
        return;
      } else {
        console.warn('[App] Token geçersiz - kullanıcı bilgisi çıkarılamadı');
        clearToken();
      }
    } else {
      console.warn('[App] Token bulunamadı veya süresi dolmuş');
    }
    
    // No valid token, show intro
    setAuthUser(null);
    setShowIntro(true);
  };

  const handleGetStarted = () => {
    setShowIntro(false);
    setShowMainApp(true);
  };

  const handleLoginSuccess = (token) => {
    setToken(token);
    const user = getUserFromToken(token);
    setAuthUser(user);
    setShowIntro(false);
    setShowMainApp(true);
  };

  const handleLogout = () => {
    console.log('App.js handleLogout called');
    console.log('Before: showMainApp =', showMainApp, 'showIntro =', showIntro);
    clearToken();
    setAuthUser(null);
    setShowMainApp(false);
    setShowIntro(true);
    console.log('After: showMainApp = false, showIntro = true');
  };

  // Special pages (email confirmation, reset password)
  if (currentPage === 'email-confirmation') {
    return <EmailConfirmationPage />;
  }

  if (currentPage === 'reset-password') {
    return <ResetPasswordPage />;
  }

  if (isLoading) {
    return <LoadingPage onLoadingComplete={handleLoadingComplete} />;
  }

  if (showIntro) {
    return <IntroPage onGetStarted={handleGetStarted} onLoginSuccess={handleLoginSuccess} />;
  }

  if (showMainApp) {
    return <DashboardPage onBack={() => setShowMainApp(false)} onLogout={handleLogout} authUser={authUser} />;
  }

  return null;
}

export default App;
