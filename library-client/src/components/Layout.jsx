import React from 'react';
import { NavLink } from 'react-router-dom';
import { LayoutDashboard, BookOpen, Users, UserCircle, ArrowLeftRight, Tags } from 'lucide-react';

const navItems = [
  { path: '/', label: 'Dashboard', icon: LayoutDashboard },
  { path: '/books', label: 'Books', icon: BookOpen },
  { path: '/authors', label: 'Authors', icon: UserCircle },
  { path: '/categories', label: 'Categories', icon: Tags },
  { path: '/members', label: 'Members', icon: Users },
  { path: '/loans', label: 'Loans', icon: ArrowLeftRight },
];

function Layout({ children }) {
  return (
    <div className="app-layout">
      <aside className="sidebar">
        <div className="sidebar-brand">
          <h1>📚 LibraryMS</h1>
          <p>Management System</p>
        </div>
        <nav className="sidebar-nav">
          {navItems.map(({ path, label, icon: Icon }) => (
            <NavLink
              key={path}
              to={path}
              end={path === '/'}
              className={({ isActive }) => `nav-link ${isActive ? 'active' : ''}`}
            >
              <Icon />
              {label}
            </NavLink>
          ))}
        </nav>
      </aside>
      <main className="main-content">
        {children}
      </main>
    </div>
  );
}

export default React.memo(Layout);
