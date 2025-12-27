import React, { useState, useEffect, useMemo } from 'react';
import './AdminDashboardPage.css';
import logo from '../../assets/logos/logo.png';
import menuIcon from '../../assets/icons/menu.png';
import profileIcon from '../../assets/icons/profile.png';
import { smtiaApi } from '../../services/smtiaApi';

const AdminDashboardPage = ({ onBack, onLogout, authUser = null }) => {
  const [activeTab, setActiveTab] = useState('overview'); // overview, users, audit-logs, serilog-logs, medicines
  const [isLoading, setIsLoading] = useState(true);
  const [userInfo, setUserInfo] = useState({
    name: 'Admin',
    email: '',
    id: ''
  });
  const [showMobileMenu, setShowMobileMenu] = useState(false);
  const [showProfileDropdown, setShowProfileDropdown] = useState(false);

  // Stats
  const [stats, setStats] = useState({
    totalUsers: 0,
    totalAdmins: 0,
    totalMedicines: 0,
    totalAuditLogs: 0,
    recentLogsLast7Days: 0
  });

  // Users
  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [usersLoading, setUsersLoading] = useState(false);
  const [userSearch, setUserSearch] = useState('');
  const [selectedUser, setSelectedUser] = useState(null);
  const [showUserModal, setShowUserModal] = useState(false);

  // Audit Logs
  const [auditLogs, setAuditLogs] = useState([]);
  const [auditLogsLoading, setAuditLogsLoading] = useState(false);
  const [auditLogsPage, setAuditLogsPage] = useState(1);
  const [auditLogsTotalPages, setAuditLogsTotalPages] = useState(1);
  const [auditLogsFilters, setAuditLogsFilters] = useState({
    startDate: '',
    endDate: '',
    action: '',
    entityType: '',
    userId: ''
  });

  // Serilog Logs
  const [serilogLogs, setSerilogLogs] = useState([]);
  const [serilogLogsLoading, setSerilogLogsLoading] = useState(false);
  const [serilogLines, setSerilogLines] = useState(1000);

  // Medicines
  const [medicines, setMedicines] = useState([]);
  const [medicinesLoading, setMedicinesLoading] = useState(false);
  const [medicineSearch, setMedicineSearch] = useState('');
  const [medForm, setMedForm] = useState({
    name: '',
    description: '',
    dosageForm: '',
    activeIngredient: '',
    manufacturer: '',
    barcode: ''
  });
  const [medSaving, setMedSaving] = useState(false);

  const filteredUsers = useMemo(() => {
    const q = userSearch.trim().toLowerCase();
    if (!q) return users;
    return users.filter(u =>
      (u.userName || '').toLowerCase().includes(q) ||
      (u.email || '').toLowerCase().includes(q) ||
      (u.fullName || '').toLowerCase().includes(q)
    );
  }, [users, userSearch]);

  const filteredMedicines = useMemo(() => {
    const q = medicineSearch.trim().toLowerCase();
    if (!q) return medicines;
    return medicines.filter(m =>
      (m.name || '').toLowerCase().includes(q) ||
      (m.activeIngredient || '').toLowerCase().includes(q) ||
      (m.manufacturer || '').toLowerCase().includes(q)
    );
  }, [medicines, medicineSearch]);

  // Load initial data
  useEffect(() => {
    const loadInitialData = async () => {
      setIsLoading(true);
      try {
        const [profile, statsData] = await Promise.all([
          smtiaApi.profile.me().catch(() => null),
          smtiaApi.admin.getStats().catch(() => null)
        ]);

        if (profile) {
          setUserInfo({
            name: profile.name || 'Admin',
            email: profile.email || '',
            id: profile.id || ''
          });
        }

        if (statsData) {
          setStats(statsData);
        }
      } catch (error) {
        console.error('Error loading initial data:', error);
      } finally {
        setIsLoading(false);
      }
    };

    loadInitialData();
  }, []);

  // Load users
  useEffect(() => {
    if (activeTab === 'users') {
      loadUsers();
    }
  }, [activeTab]);

  const loadUsers = async () => {
    setUsersLoading(true);
    try {
      const [usersData, rolesData] = await Promise.all([
        smtiaApi.admin.getUsers(),
        smtiaApi.admin.getRoles()
      ]);
      setUsers(usersData || []);
      setRoles(rolesData || []);
    } catch (error) {
      console.error('Error loading users:', error);
      alert('Kullanıcılar yüklenemedi: ' + (error?.message || 'Bilinmeyen hata'));
    } finally {
      setUsersLoading(false);
    }
  };

  // Load audit logs
  useEffect(() => {
    if (activeTab === 'audit-logs') {
      loadAuditLogs();
    }
  }, [activeTab, auditLogsPage, auditLogsFilters]);

  const loadAuditLogs = async () => {
    setAuditLogsLoading(true);
    try {
      const params = {
        page: auditLogsPage,
        pageSize: 50
      };
      if (auditLogsFilters.startDate) params.startDate = auditLogsFilters.startDate;
      if (auditLogsFilters.endDate) params.endDate = auditLogsFilters.endDate;
      if (auditLogsFilters.action) params.action = auditLogsFilters.action;
      if (auditLogsFilters.entityType) params.entityType = auditLogsFilters.entityType;
      if (auditLogsFilters.userId) params.userId = auditLogsFilters.userId;

      const data = await smtiaApi.admin.getAuditLogs(params);
      setAuditLogs(data.logs || []);
      setAuditLogsTotalPages(data.totalPages || 1);
    } catch (error) {
      console.error('Error loading audit logs:', error);
      alert('Audit logları yüklenemedi: ' + (error?.message || 'Bilinmeyen hata'));
    } finally {
      setAuditLogsLoading(false);
    }
  };

  // Load Serilog logs
  useEffect(() => {
    if (activeTab === 'serilog-logs') {
      loadSerilogLogs();
    }
  }, [activeTab, serilogLines]);

  const loadSerilogLogs = async () => {
    setSerilogLogsLoading(true);
    try {
      const data = await smtiaApi.admin.getSerilogLogs(serilogLines);
      setSerilogLogs(data.logs || []);
    } catch (error) {
      console.error('Error loading Serilog logs:', error);
      alert('Serilog dosyaları yüklenemedi: ' + (error?.message || 'Bilinmeyen hata'));
    } finally {
      setSerilogLogsLoading(false);
    }
  };

  // Load medicines
  useEffect(() => {
    if (activeTab === 'medicines') {
      loadMedicines();
    }
  }, [activeTab]);

  const loadMedicines = async () => {
    setMedicinesLoading(true);
    try {
      const res = await smtiaApi.medicines.getAll();
      setMedicines(Array.isArray(res) ? res : (res?.medicines || []));
    } catch (error) {
      console.error('Error loading medicines:', error);
      alert('İlaçlar yüklenemedi: ' + (error?.message || 'Bilinmeyen hata'));
    } finally {
      setMedicinesLoading(false);
    }
  };

  const toggleUserRole = async (userId, role) => {
    const user = users.find(u => u.id === userId);
    if (!user) return;

    const current = user.roles || [];
    const has = current.includes(role);
    const next = has ? current.filter(r => r !== role) : [...current, role];

    try {
      await smtiaApi.admin.setUserRoles(userId, next);
      setUsers(prev => prev.map(u => (u.id === userId ? { ...u, roles: next } : u)));
      alert('Rol güncellendi');
    } catch (e) {
      alert('Rol güncellenemedi: ' + (e?.message || 'Bilinmeyen hata'));
    }
  };

  const handleDeleteUser = async (userId, userName) => {
    if (!window.confirm(`"${userName}" kullanıcısını silmek istediğinizden emin misiniz? Bu işlem geri alınamaz!`)) {
      return;
    }

    try {
      await smtiaApi.admin.deleteUser(userId);
      alert('Kullanıcı silindi');
      loadUsers();
    } catch (error) {
      alert('Kullanıcı silinemedi: ' + (error?.message || 'Bilinmeyen hata'));
    }
  };

  const handleToggleLockout = async (userId, userName, isLocked) => {
    try {
      await smtiaApi.admin.toggleUserLockout(userId, !isLocked);
      alert(isLocked ? 'Kullanıcı kilidi açıldı' : 'Kullanıcı kilitlendi');
      loadUsers();
    } catch (error) {
      alert('Kullanıcı kilidi güncellenemedi: ' + (error?.message || 'Bilinmeyen hata'));
    }
  };

  const handleCreateMedicine = async () => {
    if (!medForm.name.trim()) {
      alert('İlaç adı zorunludur');
      return;
    }
    setMedSaving(true);
    try {
      await smtiaApi.medicines.create({
        name: medForm.name.trim(),
        description: medForm.description || null,
        dosageForm: medForm.dosageForm || null,
        activeIngredient: medForm.activeIngredient || null,
        manufacturer: medForm.manufacturer || null,
        barcode: medForm.barcode || null
      });

      await loadMedicines();
      setMedForm({
        name: '',
        description: '',
        dosageForm: '',
        activeIngredient: '',
        manufacturer: '',
        barcode: ''
      });
      alert('İlaç eklendi');
    } catch (e) {
      alert('İlaç eklenemedi: ' + (e?.message || 'Bilinmeyen hata'));
    } finally {
      setMedSaving(false);
    }
  };

  const handleDeleteMedicine = async (id) => {
    if (!window.confirm('Bu ilacı silmek istediğinizden emin misiniz?')) return;
    try {
      await smtiaApi.medicines.delete(id);
      await loadMedicines();
      alert('İlaç silindi');
    } catch (e) {
      alert('İlaç silinemedi: ' + (e?.message || 'Bilinmeyen hata'));
    }
  };

  const handleLogout = async () => {
    try {
      localStorage.removeItem('completedMedicines');
      if (onLogout) onLogout();
      else window.location.href = '/';
    } catch (error) {
      console.error('Logout error:', error);
      alert('Çıkış işlemi sırasında hata oluştu.');
    }
  };

  const handleBack = () => {
    if (onBack) onBack();
  };

  return (
    <div className="admin-dashboard-container">
      {/* Header */}
      <div className="admin-dashboard-header">
        <div className="admin-dashboard-header-left">
          <button className="admin-hamburger-menu" onClick={() => setShowMobileMenu(!showMobileMenu)}>
            <img src={menuIcon} alt="Menu" />
          </button>
          <div className="admin-logo">
            <img src={logo} alt="SMTIA" />
          </div>
          <div className="admin-header-title">
            <h1>Admin Dashboard</h1>
            <p>Sistem Yönetimi</p>
          </div>
        </div>
        <div className="admin-dashboard-header-right">
          <div className="admin-profile-wrapper">
            <button className="admin-profile-button" onClick={() => setShowProfileDropdown(!showProfileDropdown)}>
              <img src={profileIcon} alt="Profile" />
              <span>{userInfo.name}</span>
            </button>
            {showProfileDropdown && (
              <div className="admin-profile-dropdown">
                <button onClick={handleLogout}>Çıkış Yap</button>
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Mobile Menu */}
      {showMobileMenu && (
        <div className="admin-mobile-menu-overlay" onClick={() => setShowMobileMenu(false)}>
          <div className="admin-mobile-menu" onClick={(e) => e.stopPropagation()}>
            <div className="admin-mobile-menu-header">
              <h3>Menü</h3>
              <button onClick={() => setShowMobileMenu(false)}>✕</button>
            </div>
            <div className="admin-mobile-menu-items">
              <button onClick={() => { setActiveTab('overview'); setShowMobileMenu(false); }}>Genel Bakış</button>
              <button onClick={() => { setActiveTab('users'); setShowMobileMenu(false); }}>Kullanıcılar</button>
              <button onClick={() => { setActiveTab('audit-logs'); setShowMobileMenu(false); }}>Audit Logları</button>
              <button onClick={() => { setActiveTab('serilog-logs'); setShowMobileMenu(false); }}>Serilog Logları</button>
              <button onClick={() => { setActiveTab('medicines'); setShowMobileMenu(false); }}>İlaçlar</button>
              <button onClick={handleLogout}>Çıkış Yap</button>
            </div>
          </div>
        </div>
      )}

      {/* Main Content */}
      <div className="admin-dashboard-content">
        {/* Sidebar */}
        <div className="admin-sidebar">
          <button 
            className={`admin-sidebar-item ${activeTab === 'overview' ? 'active' : ''}`}
            onClick={() => setActiveTab('overview')}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="3" y="3" width="7" height="7"></rect>
              <rect x="14" y="3" width="7" height="7"></rect>
              <rect x="14" y="14" width="7" height="7"></rect>
              <rect x="3" y="14" width="7" height="7"></rect>
            </svg>
            <span>Genel Bakış</span>
          </button>
          <button 
            className={`admin-sidebar-item ${activeTab === 'users' ? 'active' : ''}`}
            onClick={() => setActiveTab('users')}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
              <circle cx="9" cy="7" r="4"></circle>
              <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
              <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
            </svg>
            <span>Kullanıcılar</span>
          </button>
          <button 
            className={`admin-sidebar-item ${activeTab === 'audit-logs' ? 'active' : ''}`}
            onClick={() => setActiveTab('audit-logs')}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
              <polyline points="10 9 9 9 8 9"></polyline>
            </svg>
            <span>Audit Logları</span>
          </button>
          <button 
            className={`admin-sidebar-item ${activeTab === 'serilog-logs' ? 'active' : ''}`}
            onClick={() => setActiveTab('serilog-logs')}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
              <polyline points="14 2 14 8 20 8"></polyline>
              <line x1="16" y1="13" x2="8" y2="13"></line>
              <line x1="16" y1="17" x2="8" y2="17"></line>
            </svg>
            <span>Serilog Logları</span>
          </button>
          <button 
            className={`admin-sidebar-item ${activeTab === 'medicines' ? 'active' : ''}`}
            onClick={() => setActiveTab('medicines')}
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
              <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
              <line x1="9" y1="3" x2="9" y2="21"></line>
              <line x1="15" y1="3" x2="15" y2="21"></line>
              <line x1="3" y1="9" x2="21" y2="9"></line>
              <line x1="3" y1="15" x2="21" y2="15"></line>
            </svg>
            <span>İlaçlar</span>
          </button>
        </div>

        {/* Content Area */}
        <div className="admin-content-area">
          {/* Overview Tab */}
          {activeTab === 'overview' && (
            <div className="admin-overview">
              <h2>Genel Bakış</h2>
              <div className="admin-stats-grid">
                <div className="admin-stat-card">
                  <div className="admin-stat-icon" style={{ background: '#E3F2FD' }}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#1976D2" strokeWidth="2">
                      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                      <circle cx="9" cy="7" r="4"></circle>
                      <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                      <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                    </svg>
                  </div>
                  <div className="admin-stat-content">
                    <h3>Toplam Kullanıcı</h3>
                    <p className="admin-stat-value">{stats.totalUsers}</p>
                  </div>
                </div>

                <div className="admin-stat-card">
                  <div className="admin-stat-icon" style={{ background: '#FFF3E0' }}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#F57C00" strokeWidth="2">
                      <path d="M12 2L2 7l10 5 10-5-10-5z"></path>
                      <path d="M2 17l10 5 10-5"></path>
                      <path d="M2 12l10 5 10-5"></path>
                    </svg>
                  </div>
                  <div className="admin-stat-content">
                    <h3>Admin Sayısı</h3>
                    <p className="admin-stat-value">{stats.totalAdmins}</p>
                  </div>
                </div>

                <div className="admin-stat-card">
                  <div className="admin-stat-icon" style={{ background: '#E8F5E9' }}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#388E3C" strokeWidth="2">
                      <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                      <line x1="9" y1="3" x2="9" y2="21"></line>
                      <line x1="15" y1="3" x2="15" y2="21"></line>
                    </svg>
                  </div>
                  <div className="admin-stat-content">
                    <h3>Toplam İlaç</h3>
                    <p className="admin-stat-value">{stats.totalMedicines}</p>
                  </div>
                </div>

                <div className="admin-stat-card">
                  <div className="admin-stat-icon" style={{ background: '#FCE4EC' }}>
                    <svg width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="#C2185B" strokeWidth="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                      <polyline points="14 2 14 8 20 8"></polyline>
                      <line x1="16" y1="13" x2="8" y2="13"></line>
                      <line x1="16" y1="17" x2="8" y2="17"></line>
                    </svg>
                  </div>
                  <div className="admin-stat-content">
                    <h3>Audit Logları</h3>
                    <p className="admin-stat-value">{stats.totalAuditLogs}</p>
                    <p className="admin-stat-sub">Son 7 gün: {stats.recentLogsLast7Days}</p>
                  </div>
                </div>
              </div>

              <div className="admin-quick-actions">
                <h3>Hızlı İşlemler</h3>
                <div className="admin-quick-actions-grid">
                  <button onClick={() => setActiveTab('users')} className="admin-quick-action-btn">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M17 21v-2a4 4 0 0 0-4-4H5a4 4 0 0 0-4 4v2"></path>
                      <circle cx="9" cy="7" r="4"></circle>
                      <path d="M23 21v-2a4 4 0 0 0-3-3.87"></path>
                      <path d="M16 3.13a4 4 0 0 1 0 7.75"></path>
                    </svg>
                    <span>Kullanıcıları Yönet</span>
                  </button>
                  <button onClick={() => setActiveTab('audit-logs')} className="admin-quick-action-btn">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                      <polyline points="14 2 14 8 20 8"></polyline>
                    </svg>
                    <span>Audit Logları Görüntüle</span>
                  </button>
                  <button onClick={() => setActiveTab('serilog-logs')} className="admin-quick-action-btn">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <path d="M14 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V8z"></path>
                      <polyline points="14 2 14 8 20 8"></polyline>
                    </svg>
                    <span>Serilog Logları Görüntüle</span>
                  </button>
                  <button onClick={() => setActiveTab('medicines')} className="admin-quick-action-btn">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                      <rect x="3" y="3" width="18" height="18" rx="2" ry="2"></rect>
                    </svg>
                    <span>İlaçları Yönet</span>
                  </button>
                </div>
              </div>
            </div>
          )}

          {/* Users Tab */}
          {activeTab === 'users' && (
            <div className="admin-users-tab">
              <div className="admin-tab-header">
                <h2>Kullanıcı Yönetimi</h2>
                <div className="admin-search-box">
                  <input
                    type="text"
                    placeholder="Kullanıcı ara (username, email, isim)..."
                    value={userSearch}
                    onChange={(e) => setUserSearch(e.target.value)}
                  />
                </div>
              </div>

              {usersLoading ? (
                <div className="admin-loading">Yükleniyor...</div>
              ) : (
                <div className="admin-users-list">
                  {filteredUsers.map(user => {
                    const isLocked = user.lockoutEnd && new Date(user.lockoutEnd) > new Date();
                    const lockoutDate = user.lockoutEnd ? new Date(user.lockoutEnd).toISOString() : null;
                    return (
                      <div key={user.id} className="admin-user-card">
                        <div className="admin-user-info">
                          <div className="admin-user-main">
                            <h3>{user.userName || user.email}</h3>
                            {user.fullName && <p className="admin-user-name">{user.fullName}</p>}
                            <p className="admin-user-email">{user.email}</p>
                            <div className="admin-user-meta">
                              {user.emailConfirmed ? (
                                <span className="admin-badge admin-badge-success">Email Onaylı</span>
                              ) : (
                                <span className="admin-badge admin-badge-warning">Email Onaylanmamış</span>
                              )}
                              {isLocked && <span className="admin-badge admin-badge-danger">Kilitli</span>}
                              {user.roles?.includes('Admin') && <span className="admin-badge admin-badge-info">Admin</span>}
                            </div>
                            {user.heightCm && user.weight && (
                              <p className="admin-user-details">Boy: {user.heightCm} cm, Kilo: {user.weight} kg</p>
                            )}
                            {user.bloodType && (
                              <p className="admin-user-details">Kan Grubu: {user.bloodType}</p>
                            )}
                            <p className="admin-user-details">Reçete: {user.prescriptionCount}, İlaç: {user.medicineCount}</p>
                          </div>
                        </div>
                        <div className="admin-user-actions">
                          <div className="admin-user-roles">
                            <label>Roller:</label>
                            <div className="admin-roles-list">
                              {roles.map(role => (
                                <button
                                  key={role}
                                  className={`admin-role-pill ${user.roles?.includes(role) ? 'active' : ''}`}
                                  onClick={() => toggleUserRole(user.id, role)}
                                >
                                  {role}
                                </button>
                              ))}
                            </div>
                          </div>
                          <div className="admin-user-buttons">
                            <button
                              className="admin-btn admin-btn-secondary"
                              onClick={() => handleToggleLockout(user.id, user.userName, isLocked)}
                            >
                              {isLocked ? 'Kilidi Aç' : 'Kilitle'}
                            </button>
                            <button
                              className="admin-btn admin-btn-danger"
                              onClick={() => handleDeleteUser(user.id, user.userName)}
                            >
                              Sil
                            </button>
                          </div>
                        </div>
                      </div>
                    );
                  })}
                  {filteredUsers.length === 0 && (
                    <div className="admin-empty-state">Kullanıcı bulunamadı</div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Audit Logs Tab */}
          {activeTab === 'audit-logs' && (
            <div className="admin-audit-logs-tab">
              <div className="admin-tab-header">
                <h2>Audit Logları</h2>
                <button className="admin-btn admin-btn-primary" onClick={loadAuditLogs}>Yenile</button>
              </div>

              <div className="admin-filters">
                <input
                  type="date"
                  placeholder="Başlangıç Tarihi"
                  value={auditLogsFilters.startDate}
                  onChange={(e) => setAuditLogsFilters({ ...auditLogsFilters, startDate: e.target.value })}
                />
                <input
                  type="date"
                  placeholder="Bitiş Tarihi"
                  value={auditLogsFilters.endDate}
                  onChange={(e) => setAuditLogsFilters({ ...auditLogsFilters, endDate: e.target.value })}
                />
                <input
                  type="text"
                  placeholder="Aksiyon (örn: CREATE, UPDATE)"
                  value={auditLogsFilters.action}
                  onChange={(e) => setAuditLogsFilters({ ...auditLogsFilters, action: e.target.value })}
                />
                <input
                  type="text"
                  placeholder="Entity Type (örn: Medicine, User)"
                  value={auditLogsFilters.entityType}
                  onChange={(e) => setAuditLogsFilters({ ...auditLogsFilters, entityType: e.target.value })}
                />
                <input
                  type="text"
                  placeholder="User ID"
                  value={auditLogsFilters.userId}
                  onChange={(e) => setAuditLogsFilters({ ...auditLogsFilters, userId: e.target.value })}
                />
                <button className="admin-btn admin-btn-primary" onClick={() => { setAuditLogsPage(1); loadAuditLogs(); }}>Filtrele</button>
                <button className="admin-btn admin-btn-secondary" onClick={() => {
                  setAuditLogsFilters({ startDate: '', endDate: '', action: '', entityType: '', userId: '' });
                  setAuditLogsPage(1);
                }}>Temizle</button>
              </div>

              {auditLogsLoading ? (
                <div className="admin-loading">Yükleniyor...</div>
              ) : (
                <>
                  <div className="admin-logs-table">
                    <table>
                      <thead>
                        <tr>
                          <th>Tarih</th>
                          <th>Kullanıcı</th>
                          <th>Aksiyon</th>
                          <th>Entity Type</th>
                          <th>Method</th>
                          <th>Path</th>
                          <th>Status</th>
                          <th>IP</th>
                        </tr>
                      </thead>
                      <tbody>
                        {auditLogs.map(log => (
                          <tr key={log.id}>
                            <td>{new Date(log.createdAt).toLocaleString('tr-TR')}</td>
                            <td>{log.userName}</td>
                            <td><span className={`admin-log-action admin-log-action-${log.action?.toLowerCase()}`}>{log.action}</span></td>
                            <td>{log.entityType}</td>
                            <td>{log.requestMethod}</td>
                            <td className="admin-log-path">{log.requestPath}</td>
                            <td>{log.responseStatus}</td>
                            <td>{log.ipAddress}</td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>
                  {auditLogs.length === 0 && (
                    <div className="admin-empty-state">Log bulunamadı</div>
                  )}
                  <div className="admin-pagination">
                    <button
                      className="admin-btn admin-btn-secondary"
                      disabled={auditLogsPage === 1}
                      onClick={() => setAuditLogsPage(prev => Math.max(1, prev - 1))}
                    >
                      Önceki
                    </button>
                    <span>Sayfa {auditLogsPage} / {auditLogsTotalPages}</span>
                    <button
                      className="admin-btn admin-btn-secondary"
                      disabled={auditLogsPage >= auditLogsTotalPages}
                      onClick={() => setAuditLogsPage(prev => prev + 1)}
                    >
                      Sonraki
                    </button>
                  </div>
                </>
              )}
            </div>
          )}

          {/* Serilog Logs Tab */}
          {activeTab === 'serilog-logs' && (
            <div className="admin-serilog-logs-tab">
              <div className="admin-tab-header">
                <h2>Serilog Logları</h2>
                <div className="admin-serilog-controls">
                  <input
                    type="number"
                    placeholder="Satır sayısı"
                    value={serilogLines}
                    onChange={(e) => setSerilogLines(parseInt(e.target.value) || 1000)}
                    min="100"
                    max="10000"
                  />
                  <button className="admin-btn admin-btn-primary" onClick={loadSerilogLogs}>Yenile</button>
                </div>
              </div>

              {serilogLogsLoading ? (
                <div className="admin-loading">Yükleniyor...</div>
              ) : (
                <div className="admin-serilog-content">
                  <div className="admin-serilog-logs">
                    {serilogLogs.map((line, index) => (
                      <div key={index} className="admin-serilog-line">
                        {line}
                      </div>
                    ))}
                  </div>
                  {serilogLogs.length === 0 && (
                    <div className="admin-empty-state">Log bulunamadı</div>
                  )}
                </div>
              )}
            </div>
          )}

          {/* Medicines Tab */}
          {activeTab === 'medicines' && (
            <div className="admin-medicines-tab">
              <div className="admin-tab-header">
                <h2>İlaç Yönetimi</h2>
                <div className="admin-search-box">
                  <input
                    type="text"
                    placeholder="İlaç ara (ad, etken madde, üretici)..."
                    value={medicineSearch}
                    onChange={(e) => setMedicineSearch(e.target.value)}
                  />
                </div>
              </div>

              <div className="admin-medicines-grid">
                <div className="admin-medicine-form-card">
                  <h3>Yeni İlaç Ekle</h3>
                  <div className="admin-form">
                    <input
                      placeholder="İlaç adı *"
                      value={medForm.name}
                      onChange={e => setMedForm({ ...medForm, name: e.target.value })}
                    />
                    <input
                      placeholder="Etken madde"
                      value={medForm.activeIngredient}
                      onChange={e => setMedForm({ ...medForm, activeIngredient: e.target.value })}
                    />
                    <input
                      placeholder="Dozaj formu"
                      value={medForm.dosageForm}
                      onChange={e => setMedForm({ ...medForm, dosageForm: e.target.value })}
                    />
                    <input
                      placeholder="Üretici"
                      value={medForm.manufacturer}
                      onChange={e => setMedForm({ ...medForm, manufacturer: e.target.value })}
                    />
                    <input
                      placeholder="Barkod"
                      value={medForm.barcode}
                      onChange={e => setMedForm({ ...medForm, barcode: e.target.value })}
                    />
                    <textarea
                      placeholder="Açıklama"
                      value={medForm.description}
                      onChange={e => setMedForm({ ...medForm, description: e.target.value })}
                      rows="4"
                    />
                    <button
                      className="admin-btn admin-btn-primary"
                      onClick={handleCreateMedicine}
                      disabled={medSaving}
                    >
                      {medSaving ? 'Kaydediliyor...' : 'Ekle'}
                    </button>
                  </div>
                </div>

                <div className="admin-medicines-list-card">
                  <h3>İlaçlar ({filteredMedicines.length})</h3>
                  {medicinesLoading ? (
                    <div className="admin-loading">Yükleniyor...</div>
                  ) : (
                    <div className="admin-medicines-list">
                      {filteredMedicines.slice(0, 100).map(medicine => (
                        <div key={medicine.id} className="admin-medicine-item">
                          <div className="admin-medicine-info">
                            <strong>{medicine.name}</strong>
                            {medicine.activeIngredient && (
                              <p className="admin-medicine-sub">Etken: {medicine.activeIngredient}</p>
                            )}
                            {medicine.manufacturer && (
                              <p className="admin-medicine-sub">Üretici: {medicine.manufacturer}</p>
                            )}
                          </div>
                          <button
                            className="admin-btn admin-btn-danger admin-btn-sm"
                            onClick={() => handleDeleteMedicine(medicine.id)}
                          >
                            Sil
                          </button>
                        </div>
                      ))}
                      {filteredMedicines.length > 100 && (
                        <div className="admin-state">İlk 100 kayıt gösteriliyor (filtreyi daraltın)</div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default AdminDashboardPage;

