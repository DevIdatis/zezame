import { Component, OnInit, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  LucideAngularModule,
  Settings as SettingsIcon,
  User,
  Bell,
  Palette,
  Shield,
  CreditCard,
  Globe,
  Moon,
  Sun,
  Monitor,
  Check,
  ChevronRight,
  Camera,
  Mail,
  Lock,
  Smartphone,
  Trash2,
  Download,
  LucideIconData,
} from 'lucide-angular';
import { UserService } from 'services/user.service';
import { AuthService } from 'services/auth.service';
import { ThemeService, Theme } from 'services/theme.service';
import { ErrorModalService } from 'services/error-modal.service';
import { AuthorizationService } from 'services/authorization.service';

type TabId = 'profile' | 'notifications' | 'appearance' | 'security' | 'billing';

interface Tab {
  id: TabId;
  label: string;
  icon: LucideIconData;
  canSee?: boolean;
}

interface NotificationPrefs {
  email: boolean;
  push: boolean;
  weekly: boolean;
  mentions: boolean;
}

@Component({
  selector: 'app-settings',
  imports: [LucideAngularModule],
  templateUrl: './settings.html',
  styleUrl: './settings.scss',
})
export class Settings implements OnInit {
  private authorization = inject(AuthorizationService);
  private userService = inject(UserService);
  private auth = inject(AuthService);
  private themeService = inject(ThemeService);
  private errorModal = inject(ErrorModalService);
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);

  icons = {
    SettingsIcon,
    User,
    Bell,
    Palette,
    Shield,
    CreditCard,
    Globe,
    Moon,
    Sun,
    Monitor,
    Check,
    ChevronRight,
    Camera,
    Mail,
    Lock,
    Smartphone,
    Trash2,
    Download,
  };

  activeTab = signal<TabId>('profile');
  readonly isOwner = computed(() =>
    this.authorization.isOwner()
  );
  readonly tabs: Tab[] = [
    { id: 'profile', label: 'Perfil', icon: User, canSee: true },
    { id: 'notifications', label: 'Notificaciones', icon: Bell, canSee: true },
    { id: 'appearance', label: 'Apariencia', icon: Palette, canSee: true },
    { id: 'security', label: 'Seguridad', icon: Shield, canSee: true },
    { id: 'billing', label: 'Facturación', icon: CreditCard, canSee: this.isOwner() },
  ];

  // ===== Perfil =====
  name = signal('');
  email = signal('');
  timezone = signal('');
  language = signal('');
  savingProfile = signal(false);
  profileSaved = signal(false);

  avatarUrl = signal(
    'https://api.dicebear.com/7.x/avataaars/svg?seed=zezame&backgroundColor=ffdfbf',
  );

  // ===== Apariencia =====
  readonly themeOptions: { id: Theme; label: string; icon: LucideIconData }[] = [
    { id: 'light', label: 'Claro', icon: Sun },
    { id: 'dark', label: 'Oscuro', icon: Moon },
    { id: 'system', label: 'Sistema', icon: Monitor },
  ];
  theme = this.themeService.theme;
  readonly accents: { name: string; class: string; ring: string }[] = [
    { name: 'coral', class: 'bg-coral', ring: 'ring-coral' },
    { name: 'cyan', class: 'bg-cyan', ring: 'ring-cyan' },
    { name: 'yellow', class: 'bg-yellow', ring: 'ring-yellow' },
    { name: 'lime', class: 'bg-lime', ring: 'ring-lime' },
    { name: 'purple', class: 'bg-purple', ring: 'ring-purple' },
    { name: 'pink', class: 'bg-pink-400', ring: 'ring-pink-400' },
  ];
  accent = signal('coral');

  // ===== Notificaciones =====
  notifications = signal<NotificationPrefs>({
    email: true,
    push: true,
    weekly: true,
    mentions: false,
  });
  readonly notificationItems: {
    id: keyof NotificationPrefs;
    label: string;
    description: string;
    icon: LucideIconData;
  }[] = [
    {
      id: 'email',
      label: 'Notificaciones por email',
      description: 'Recibe un resumen diario de tu actividad',
      icon: Mail,
    },
    {
      id: 'push',
      label: 'Notificaciones push',
      description: 'Recibe alertas en tiempo real en tu navegador',
      icon: Bell,
    },
    {
      id: 'weekly',
      label: 'Resumen semanal',
      description: 'Recibe un informe cada lunes con tus estadísticas',
      icon: Globe,
    },
    {
      id: 'mentions',
      label: 'Solo menciones',
      description: 'Solo notificar cuando alguien te mencione',
      icon: Smartphone,
    },
  ];

  // ===== Seguridad =====
  showPasswordForm = signal(false);
  currentPassword = signal('');
  newPassword = signal('');
  changingPassword = signal(false);

  ngOnInit(): void {
    this.userService.getMe().subscribe({
      next: (p) => {
        this.name.set(p.fullName);
        this.email.set(p.email);
        this.timezone.set(p.timezone ?? 'Europe/Madrid');
        this.language.set(p.language ?? 'Español');
        this.avatarUrl.set(
          `https://api.dicebear.com/7.x/avataaars/svg?seed=${encodeURIComponent(p.id)}&backgroundColor=ffdfbf`,
        );
      },
      error: (err) => console.error('Error cargando perfil:', err),
    });

    if (this.isBrowser) {
      try {
        const notif = localStorage.getItem('notif_prefs');
        if (notif) this.notifications.set(JSON.parse(notif));
        this.accent.set(localStorage.getItem('accent') ?? 'coral');
      } catch {
        /* ignore */
      }
    }
  }

  setTab(id: TabId): void {
    this.activeTab.set(id);
  }

  // ===== Perfil =====
  saveProfile(): void {
    this.savingProfile.set(true);
    this.profileSaved.set(false);
    this.userService
      .updateMe({
        fullName: this.name(),
        email: this.email(),
        timezone: this.timezone(),
        language: this.language(),
      })
      .subscribe({
        next: (p) => {
          this.auth.patchUser({ fullName: p.fullName, email: p.email });
          this.savingProfile.set(false);
          this.profileSaved.set(true);
        },
        error: (e) => {
          this.savingProfile.set(false);
          this.errorModal.open({
            title: 'No se pudo guardar el perfil',
            message: e.error?.message ?? 'Inténtalo de nuevo',
          });
        },
      });
  }

  // ===== Apariencia =====
  setTheme(theme: Theme): void {
    this.themeService.set(theme);
  }

  setAccent(color: string): void {
    this.accent.set(color);
    if (this.isBrowser) {
      try {
        localStorage.setItem('accent', color);
      } catch {
        /* ignore */
      }
    }
  }

  // ===== Notificaciones =====
  toggleNotification(id: keyof NotificationPrefs): void {
    this.notifications.update((n) => ({ ...n, [id]: !n[id] }));
    if (this.isBrowser) {
      try {
        localStorage.setItem('notif_prefs', JSON.stringify(this.notifications()));
      } catch {
        /* ignore */
      }
    }
  }

  // ===== Seguridad =====
  submitPassword(): void {
    if (!this.currentPassword() || !this.newPassword()) return;
    this.changingPassword.set(true);
    this.userService
      .changePassword({ currentPassword: this.currentPassword(), newPassword: this.newPassword() })
      .subscribe({
        next: () => {
          this.changingPassword.set(false);
          this.showPasswordForm.set(false);
          this.currentPassword.set('');
          this.newPassword.set('');
          this.errorModal.open({
            title: 'Contraseña actualizada',
            message: 'Tu contraseña se ha cambiado correctamente.',
          });
        },
        error: (e) => {
          this.changingPassword.set(false);
          this.errorModal.open({
            title: 'No se pudo cambiar la contraseña',
            message: e.error?.message ?? 'Revisa la contraseña actual',
          });
        },
      });
  }
}
