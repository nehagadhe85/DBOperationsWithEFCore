import React from 'react';
import { useDashboard } from '../hooks/useData';
import { Loading, ErrorBanner } from '../components/StatusComponents';
import { BookOpen, Users, ArrowLeftRight, AlertTriangle, UserCircle, Tags } from 'lucide-react';

function Dashboard() {
  const { stats, loading, error } = useDashboard();

  if (loading) return <Loading />;

  return (
    <div>
      <div className="page-header">
        <h2>Dashboard</h2>
        <p>Overview of your library at a glance</p>
      </div>

      <ErrorBanner message={error} />

      {stats && (
        <>
          <div className="stats-grid">
            <div className="stat-card">
              <div className="stat-card-icon accent"><BookOpen size={22} /></div>
              <div className="stat-card-value">{stats.totalBooks}</div>
              <div className="stat-card-label">Total Books</div>
            </div>
            <div className="stat-card">
              <div className="stat-card-icon info"><Users size={22} /></div>
              <div className="stat-card-value">{stats.totalMembers}</div>
              <div className="stat-card-label">Total Members</div>
            </div>
            <div className="stat-card">
              <div className="stat-card-icon success"><ArrowLeftRight size={22} /></div>
              <div className="stat-card-value">{stats.activeLoans}</div>
              <div className="stat-card-label">Active Loans</div>
            </div>
            <div className="stat-card">
              <div className="stat-card-icon danger"><AlertTriangle size={22} /></div>
              <div className="stat-card-value">{stats.overdueLoans}</div>
              <div className="stat-card-label">Overdue Loans</div>
            </div>
            <div className="stat-card">
              <div className="stat-card-icon warning"><UserCircle size={22} /></div>
              <div className="stat-card-value">{stats.totalAuthors}</div>
              <div className="stat-card-label">Authors</div>
            </div>
            <div className="stat-card">
              <div className="stat-card-icon info"><Tags size={22} /></div>
              <div className="stat-card-value">{stats.totalCategories}</div>
              <div className="stat-card-label">Categories</div>
            </div>
          </div>

          <div className="card">
            <div className="card-header">
              <h3>Recent Loans</h3>
            </div>
            {stats.recentLoans.length > 0 ? (
              <table className="data-table">
                <thead>
                  <tr>
                    <th>Book</th>
                    <th>Member</th>
                    <th>Loan Date</th>
                    <th>Due Date</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {stats.recentLoans.map((loan) => (
                    <tr key={loan.id}>
                      <td>{loan.bookTitle}</td>
                      <td>{loan.memberName}</td>
                      <td>{new Date(loan.loanDate).toLocaleDateString()}</td>
                      <td>{new Date(loan.dueDate).toLocaleDateString()}</td>
                      <td>
                        <span className={`badge ${
                          loan.status === 'Active' ? 'badge-success' :
                          loan.status === 'Returned' ? 'badge-info' : 'badge-danger'
                        }`}>
                          {loan.status}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            ) : (
              <div className="card-body">
                <p style={{ color: 'var(--color-text-muted)', textAlign: 'center' }}>No recent loans</p>
              </div>
            )}
          </div>
        </>
      )}
    </div>
  );
}

export default Dashboard;
