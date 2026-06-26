import React, { useState, useCallback, useEffect } from 'react';
import { useLoans, useDebounce } from '../hooks/useData';
import { loansApi, booksApi, membersApi } from '../services/api';
import Modal from '../components/Modal';
import Pagination from '../components/Pagination';
import { Loading, EmptyState, ErrorBanner } from '../components/StatusComponents';
import { Plus, ArrowLeftRight, RotateCcw } from 'lucide-react';
import toast from 'react-hot-toast';

function LoansPage() {
  const { data: loans, loading, error, page, setPage, totalPages, totalCount, pageSize, refresh, updateParams } = useLoans();
  const [showCheckoutModal, setShowCheckoutModal] = useState(false);
  const [statusFilter, setStatusFilter] = useState('');
  const [checkoutForm, setCheckoutForm] = useState({ bookId: '', memberId: '', loanDurationDays: 14 });
  const [saving, setSaving] = useState(false);

  // For dropdowns in checkout modal
  const [books, setBooks] = useState([]);
  const [members, setMembers] = useState([]);

  useEffect(() => {
    updateParams({ status: statusFilter || undefined });
  }, [statusFilter, updateParams]);

  const loadDropdownData = useCallback(async () => {
    try {
      const [booksRes, membersRes] = await Promise.all([
        booksApi.getAll({ page: 1, pageSize: 100 }),
        membersApi.getAll({ page: 1, pageSize: 100 }),
      ]);
      setBooks(booksRes.data.items.filter(b => b.copiesAvailable > 0));
      setMembers(membersRes.data.items.filter(m => m.isActive));
    } catch (err) {
      toast.error('Failed to load data for checkout');
    }
  }, []);

  const openCheckout = useCallback(() => {
    setCheckoutForm({ bookId: '', memberId: '', loanDurationDays: 14 });
    loadDropdownData();
    setShowCheckoutModal(true);
  }, [loadDropdownData]);

  const handleCheckout = useCallback(async (e) => {
    e.preventDefault();
    setSaving(true);
    try {
      await loansApi.checkout({
        bookId: parseInt(checkoutForm.bookId),
        memberId: parseInt(checkoutForm.memberId),
        loanDurationDays: parseInt(checkoutForm.loanDurationDays),
      });
      toast.success('Book checked out successfully!');
      setShowCheckoutModal(false);
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Checkout failed');
    } finally {
      setSaving(false);
    }
  }, [checkoutForm, refresh]);

  const handleReturn = useCallback(async (loanId, bookTitle) => {
    if (!window.confirm(`Return "${bookTitle}"?`)) return;
    try {
      await loansApi.return(loanId);
      toast.success('Book returned successfully!');
      refresh();
    } catch (err) {
      toast.error(err.response?.data?.message || 'Return failed');
    }
  }, [refresh]);

  const handleChange = useCallback((e) => {
    const { name, value } = e.target;
    setCheckoutForm(prev => ({ ...prev, [name]: value }));
  }, []);

  if (loading && loans.length === 0) return <Loading />;

  return (
    <div>
      <div className="page-header page-header-actions">
        <div>
          <h2>Loans</h2>
          <p>Track book checkouts and returns</p>
        </div>
        <button className="btn btn-primary" onClick={openCheckout}>
          <Plus size={18} /> Checkout Book
        </button>
      </div>

      <ErrorBanner message={error} />

      <div className="toolbar">
        <select
          className="form-select filter-select"
          value={statusFilter}
          onChange={(e) => setStatusFilter(e.target.value)}
        >
          <option value="">All Statuses</option>
          <option value="Active">Active</option>
          <option value="Returned">Returned</option>
          <option value="Overdue">Overdue</option>
        </select>
      </div>

      <div className="card">
        {loans.length > 0 ? (
          <>
            <table className="data-table">
              <thead>
                <tr>
                  <th>Book</th>
                  <th>Member</th>
                  <th>Loan Date</th>
                  <th>Due Date</th>
                  <th>Return Date</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {loans.map((loan) => {
                  const isOverdue = loan.status === 'Active' && new Date(loan.dueDate) < new Date();
                  return (
                    <tr key={loan.id}>
                      <td style={{ fontWeight: 500 }}>{loan.bookTitle}</td>
                      <td>{loan.memberName}</td>
                      <td>{new Date(loan.loanDate).toLocaleDateString()}</td>
                      <td>{new Date(loan.dueDate).toLocaleDateString()}</td>
                      <td>{loan.returnDate ? new Date(loan.returnDate).toLocaleDateString() : '—'}</td>
                      <td>
                        <span className={`badge ${
                          isOverdue ? 'badge-danger' :
                          loan.status === 'Active' ? 'badge-success' :
                          'badge-info'
                        }`}>
                          {isOverdue ? 'Overdue' : loan.status}
                        </span>
                      </td>
                      <td>
                        {loan.status === 'Active' && (
                          <button
                            className="btn btn-success btn-sm"
                            onClick={() => handleReturn(loan.id, loan.bookTitle)}
                            title="Return Book"
                          >
                            <RotateCcw size={14} /> Return
                          </button>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
            <Pagination page={page} totalPages={totalPages} totalCount={totalCount} pageSize={pageSize} onPageChange={setPage} />
          </>
        ) : (
          <EmptyState icon={ArrowLeftRight} title="No loans found" message="Checkout a book to create a loan record." />
        )}
      </div>

      {/* Checkout Modal */}
      <Modal
        isOpen={showCheckoutModal}
        onClose={() => setShowCheckoutModal(false)}
        title="Checkout Book"
        footer={
          <>
            <button className="btn btn-ghost" onClick={() => setShowCheckoutModal(false)}>Cancel</button>
            <button className="btn btn-primary" onClick={handleCheckout} disabled={saving}>
              {saving ? 'Processing...' : 'Checkout'}
            </button>
          </>
        }
      >
        <form onSubmit={handleCheckout}>
          <div className="form-group">
            <label className="form-label">Book * (only available books shown)</label>
            <select className="form-select" name="bookId" value={checkoutForm.bookId} onChange={handleChange} required>
              <option value="">Select a Book</option>
              {books.map(b => (
                <option key={b.id} value={b.id}>
                  {b.title} — {b.copiesAvailable} available
                </option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Member * (only active members shown)</label>
            <select className="form-select" name="memberId" value={checkoutForm.memberId} onChange={handleChange} required>
              <option value="">Select a Member</option>
              {members.map(m => (
                <option key={m.id} value={m.id}>{m.fullName}</option>
              ))}
            </select>
          </div>
          <div className="form-group">
            <label className="form-label">Loan Duration (days)</label>
            <input className="form-input" type="number" name="loanDurationDays" value={checkoutForm.loanDurationDays} onChange={handleChange} min={1} max={90} />
          </div>
        </form>
      </Modal>
    </div>
  );
}

export default LoansPage;
