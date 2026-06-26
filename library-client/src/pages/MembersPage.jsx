import React, { useState, useEffect, useCallback } from 'react';
import { useMembers, useDebounce } from '../hooks/useData';
import { membersApi } from '../services/api';
import Modal from '../components/Modal';
import Pagination from '../components/Pagination';
import { Loading, EmptyState, ErrorBanner } from '../components/StatusComponents';
import { Plus, Search, Pencil, Trash2, Users } from 'lucide-react';
import toast from 'react-hot-toast';

const emptyForm = { fullName: '', email: '', phone: '', isActive: true };

function MembersPage() {
  const { data: members, loading, error, page, setPage, totalPages, totalCount, pageSize, refresh, updateParams } = useMembers();
  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const debouncedSearch = useDebounce(searchTerm, 400);

  useEffect(() => {
    updateParams({ search: debouncedSearch || undefined });
  }, [debouncedSearch, updateParams]);

  const openCreate = useCallback(() => {
    setEditing(null);
    setForm(emptyForm);
    setShowModal(true);
  }, []);

  const openEdit = useCallback((member) => {
    setEditing(member);
    setForm({
      fullName: member.fullName,
      email: member.email,
      phone: member.phone || '',
      isActive: member.isActive,
    });
    setShowModal(true);
  }, []);

  const handleSubmit = useCallback(async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      if (editing) {
        await membersApi.update(editing.id, form);
        toast.success('Member updated');
      } else {
        await membersApi.create(form);
        toast.success('Member created');
      }
      setShowModal(false);
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to save');
    } finally {
      setSaving(false);
    }
  }, [form, editing, refresh]);

  const handleDelete = useCallback(async (member) => {
    if (!window.confirm(`Delete member "${member.fullName}"?`)) return;
    try {
      await membersApi.delete(member.id);
      toast.success('Member deleted');
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Failed to delete');
    }
  }, [refresh]);

  const handleChange = useCallback((e) => {
    const { name, value, type, checked } = e.target;
    setForm(prev => ({ ...prev, [name]: type === 'checkbox' ? checked : value }));
  }, []);

  if (loading && members.length === 0) return <Loading />;

  return (
    <div>
      <div className="page-header page-header-actions">
        <div>
          <h2>Members</h2>
          <p>Manage library members</p>
        </div>
        <button className="btn btn-primary" onClick={openCreate}>
          <Plus size={18} /> Add Member
        </button>
      </div>

      <ErrorBanner message={error} />

      <div className="toolbar">
        <div className="search-bar">
          <Search />
          <input
            className="form-input"
            placeholder="Search members..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
          />
        </div>
      </div>

      <div className="card">
        {members.length > 0 ? (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Email</th>
                  <th>Phone</th>
                  <th>Member Since</th>
                  <th>Active Loans</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {members.map((member) => (
                  <tr key={member.id}>
                    <td style={{ fontWeight: 500 }}>{member.fullName}</td>
                    <td>{member.email}</td>
                    <td>{member.phone || '—'}</td>
                    <td>{new Date(member.membershipDate).toLocaleDateString()}</td>
                    <td><span className="badge badge-info">{member.activeLoansCount}</span></td>
                    <td>
                      <span className={`badge ${member.isActive ? 'badge-success' : 'badge-danger'}`}>
                        {member.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    <td>
                      <div className="action-btns">
                        <button className="btn btn-ghost btn-sm" onClick={() => openEdit(member)}><Pencil size={14} /></button>
                        <button className="btn btn-ghost btn-sm" onClick={() => handleDelete(member)} style={{ color: 'var(--color-danger)' }}><Trash2 size={14} /></button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
            <Pagination page={page} totalPages={totalPages} totalCount={totalCount} pageSize={pageSize} onPageChange={setPage} />
          </>
        ) : (
          <EmptyState icon={Users} title="No members found" message="Add your first member to the library." />
        )}
      </div>

      <Modal
        isOpen={showModal}
        onClose={() => setShowModal(false)}
        title={editing ? 'Edit Member' : 'Add New Member'}
        footer={
          <>
            <button className="btn btn-ghost" onClick={() => setShowModal(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleSubmit} disabled={saving}>
              {saving ? 'Saving...' : (editing ? 'Update' : 'Create')}
            </button>
          </>
        }
      >
        <form onSubmit={handleSubmit}>
          <div className="form-group">
            <label className="form-label">Full Name *</label>
            <input className="form-input" name="fullName" value={form.fullName} onChange={handleChange} required />
          </div>
          <div className="form-row">
            <div className="form-group">
              <label className="form-label">Email *</label>
              <input className="form-input" type="email" name="email" value={form.email} onChange={handleChange} required />
            </div>
            <div className="form-group">
              <label className="form-label">Phone</label>
              <input className="form-input" name="phone" value={form.phone} onChange={handleChange} />
            </div>
          </div>
          {editing && (
            <div className="form-group">
              <label style={{ display: 'flex', alignItems: 'center', gap: 'var(--space-2)', cursor: 'pointer' }}>
                <input type="checkbox" name="isActive" checked={form.isActive} onChange={handleChange} />
                <span className="form-label" style={{ marginBottom: 0 }}>Active Member</span>
              </label>
            </div>
          )}
        </form>
      </Modal>
    </div>
  );
}

export default MembersPage;
