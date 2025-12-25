import React, { useEffect, useMemo, useState } from 'react';
import './AdminPanel.css';
import '../ModeratorPanel/ModeratorPanel.css';
import { smtiaApi } from '../../services/smtiaApi';

const AdminPanel = ({ onClose }) => {
  const [tab, setTab] = useState('users'); // users | medicines

  const [users, setUsers] = useState([]);
  const [roles, setRoles] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  // Medicines state (re-using existing ModeratorPanel logic)
  const [medicines, setMedicines] = useState([]);
  const [medLoading, setMedLoading] = useState(false);
  const [medError, setMedError] = useState('');
  const [medSearch, setMedSearch] = useState('');
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
    const q = search.trim().toLowerCase();
    if (!q) return users;
    return users.filter(u =>
      (u.userName || '').toLowerCase().includes(q) ||
      (u.email || '').toLowerCase().includes(q)
    );
  }, [users, search]);

  const filteredMedicines = useMemo(() => {
    const q = medSearch.trim().toLowerCase();
    if (!q) return medicines;
    return medicines.filter(m =>
      (m.name || '').toLowerCase().includes(q) ||
      (m.activeIngredient || '').toLowerCase().includes(q) ||
      (m.manufacturer || '').toLowerCase().includes(q)
    );
  }, [medicines, medSearch]);

  useEffect(() => {
    let mounted = true;
    const load = async () => {
      setLoading(true);
      setError('');
      try {
        const [u, r] = await Promise.all([
          smtiaApi.admin.getUsers(),
          smtiaApi.admin.getRoles()
        ]);
        if (!mounted) return;
        setUsers(u || []);
        setRoles(r || []);
      } catch (e) {
        if (!mounted) return;
        setError(e?.message || 'Admin verileri yüklenemedi');
      } finally {
        if (mounted) setLoading(false);
      }
    };
    load();
    return () => { mounted = false; };
  }, []);

  useEffect(() => {
    if (tab !== 'medicines') return;
    let mounted = true;
    const loadMeds = async () => {
      setMedLoading(true);
      setMedError('');
      try {
        const res = await smtiaApi.medicines.getAll();
        if (!mounted) return;
        setMedicines(Array.isArray(res) ? res : (res?.medicines || []));
      } catch (e) {
        if (!mounted) return;
        setMedError(e?.message || 'İlaçlar yüklenemedi');
      } finally {
        if (mounted) setMedLoading(false);
      }
    };
    loadMeds();
    return () => { mounted = false; };
  }, [tab]);

  const toggleRole = async (userId, role) => {
    const user = users.find(u => u.id === userId);
    if (!user) return;

    const current = user.roles || [];
    const has = current.includes(role);
    const next = has ? current.filter(r => r !== role) : [...current, role];

    try {
      await smtiaApi.admin.setUserRoles(userId, next);
      setUsers(prev => prev.map(u => (u.id === userId ? { ...u, roles: next } : u)));
    } catch (e) {
      alert(e?.message || 'Rol güncellenemedi');
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

      const res = await smtiaApi.medicines.getAll();
      setMedicines(Array.isArray(res) ? res : (res?.medicines || []));
      setMedForm({
        name: '',
        description: '',
        dosageForm: '',
        activeIngredient: '',
        manufacturer: '',
        barcode: ''
      });
    } catch (e) {
      alert(e?.message || 'İlaç eklenemedi');
    } finally {
      setMedSaving(false);
    }
  };

  const handleDeleteMedicine = async (id) => {
    if (!window.confirm('Bu ilacı silmek istiyor musunuz?')) return;
    try {
      await smtiaApi.medicines.delete(id);
      setMedicines(prev => prev.filter(m => m.id !== id));
    } catch (e) {
      alert(e?.message || 'Silinemedi');
    }
  };

  return (
    <div className="admin-panel-overlay" onClick={onClose}>
      <div className="admin-panel" onClick={(e) => e.stopPropagation()}>
        <div className="admin-panel-header">
          <div>
            <h2>Admin Panel</h2>
            <p>Sistem yönetimi (kullanıcılar + ilaçlar)</p>
          </div>
          <button className="admin-close" onClick={onClose}>✕</button>
        </div>

        <div className="admin-tabs">
          <button className={`admin-tab ${tab === 'users' ? 'active' : ''}`} onClick={() => setTab('users')}>Kullanıcılar</button>
          <button className={`admin-tab ${tab === 'medicines' ? 'active' : ''}`} onClick={() => setTab('medicines')}>İlaçlar</button>
        </div>

        {tab === 'users' && (
          <>
            <div className="admin-toolbar">
              <input
                className="admin-search"
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                placeholder="Kullanıcı ara (username / email)"
              />
            </div>

            {loading && <div className="admin-state">Yükleniyor…</div>}
            {!loading && error && <div className="admin-error">{error}</div>}

            {!loading && !error && (
              <div className="admin-users">
                {filteredUsers.map(u => (
                  <div key={u.id} className="admin-user-card">
                    <div className="admin-user-main">
                      <div className="admin-user-title">
                        <strong>{u.userName}</strong>
                        <span className={`badge ${u.emailConfirmed ? 'ok' : 'warn'}`}>
                          {u.emailConfirmed ? 'Email confirmed' : 'Email not confirmed'}
                        </span>
                      </div>
                      <div className="admin-user-sub">{u.email}</div>
                    </div>

                    <div className="admin-roles">
                      {roles.map(role => (
                        <button
                          key={role}
                          className={`role-pill ${u.roles?.includes(role) ? 'active' : ''}`}
                          onClick={() => toggleRole(u.id, role)}
                          type="button"
                        >
                          {role}
                        </button>
                      ))}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </>
        )}

        {tab === 'medicines' && (
          <div className="mod-grid">
            <div className="mod-card">
              <h3>Yeni ilaç ekle</h3>
              <div className="mod-form">
                <input placeholder="İlaç adı *" value={medForm.name} onChange={e => setMedForm({ ...medForm, name: e.target.value })} />
                <input placeholder="Etken madde" value={medForm.activeIngredient} onChange={e => setMedForm({ ...medForm, activeIngredient: e.target.value })} />
                <input placeholder="Dozaj formu" value={medForm.dosageForm} onChange={e => setMedForm({ ...medForm, dosageForm: e.target.value })} />
                <input placeholder="Üretici" value={medForm.manufacturer} onChange={e => setMedForm({ ...medForm, manufacturer: e.target.value })} />
                <input placeholder="Barkod" value={medForm.barcode} onChange={e => setMedForm({ ...medForm, barcode: e.target.value })} />
                <textarea placeholder="Açıklama" value={medForm.description} onChange={e => setMedForm({ ...medForm, description: e.target.value })} />
                <button className="mod-primary" onClick={handleCreateMedicine} disabled={medSaving}>
                  {medSaving ? 'Kaydediliyor…' : 'Ekle'}
                </button>
              </div>
            </div>

            <div className="mod-card">
              <h3>İlaçlar</h3>
              <input
                className="mod-search"
                value={medSearch}
                onChange={(e) => setMedSearch(e.target.value)}
                placeholder="Ara (ad / etken madde / üretici)"
              />

              {medLoading && <div className="mod-state">Yükleniyor…</div>}
              {!medLoading && medError && <div className="mod-error">{medError}</div>}

              {!medLoading && !medError && (
                <div className="mod-list">
                  {filteredMedicines.slice(0, 100).map(m => (
                    <div key={m.id} className="mod-item">
                      <div className="mod-item-main">
                        <strong>{m.name}</strong>
                        <div className="mod-item-sub">
                          {m.activeIngredient ? `Etken: ${m.activeIngredient}` : '—'}
                        </div>
                      </div>
                      <button className="mod-danger" onClick={() => handleDeleteMedicine(m.id)}>Sil</button>
                    </div>
                  ))}
                  {filteredMedicines.length > 100 && (
                    <div className="mod-state">İlk 100 kayıt gösteriliyor (filtreyi daraltın).</div>
                  )}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminPanel;


