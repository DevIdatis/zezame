import { Component, OnDestroy, OnInit, PLATFORM_ID, computed, inject, signal } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  LucideAngularModule,
  Bell,
  Check,
  CheckCheck,
  Trash2,
  Sparkles,
  Target,
  Users,
  Info,
  LucideIconData,
} from 'lucide-angular';
import { NotificationService } from 'services/notification.service';
import { AppNotification } from '../../../core/models';

@Component({
  selector: 'app-notification-bell',
  imports: [LucideAngularModule],
  templateUrl: './notification-bell.html',
  styleUrl: './notification-bell.scss',
})
export class NotificationBell implements OnInit, OnDestroy {
  private service = inject(NotificationService);
  private platformId = inject(PLATFORM_ID);
  private isBrowser = isPlatformBrowser(this.platformId);
  private pollId: number | null = null;

  Bell = Bell;
  Check = Check;
  CheckCheck = CheckCheck;
  Trash2 = Trash2;

  open = signal(false);
  notifications = signal<AppNotification[]>([]);
  unread = signal(0);

  unreadLabel = computed(() => {
    const n = this.unread();
    return n > 9 ? '9+' : `${n}`;
  });

  ngOnInit(): void {
    this.refreshCount();
    if (this.isBrowser) {
      // Refresca el contador cada 45s para mantener el badge al día
      this.pollId = window.setInterval(() => this.refreshCount(), 45000);
    }
  }

  ngOnDestroy(): void {
    if (this.pollId !== null) clearInterval(this.pollId);
  }

  toggle(): void {
    const next = !this.open();
    this.open.set(next);
    if (next) this.loadList();
  }

  close(): void {
    this.open.set(false);
  }

  private refreshCount(): void {
    this.service.unreadCount().subscribe({
      next: (c) => this.unread.set(c),
      error: () => {},
    });
  }

  private loadList(): void {
    this.service.list().subscribe({
      next: (list) => this.notifications.set(list),
      error: () => {},
    });
  }

  onClick(n: AppNotification): void {
    if (n.isRead) return;
    this.service.markRead(n.id).subscribe({
      next: () => {
        this.notifications.update((list) =>
          list.map((x) => (x.id === n.id ? { ...x, isRead: true } : x)),
        );
        this.unread.update((c) => Math.max(0, c - 1));
      },
      error: () => {},
    });
  }

  markAll(): void {
    this.service.markAllRead().subscribe({
      next: () => {
        this.notifications.update((list) => list.map((x) => ({ ...x, isRead: true })));
        this.unread.set(0);
      },
      error: () => {},
    });
  }

  remove(n: AppNotification, event: Event): void {
    event.stopPropagation();
    this.service.delete(n.id).subscribe({
      next: () => {
        this.notifications.update((list) => list.filter((x) => x.id !== n.id));
        if (!n.isRead) this.unread.update((c) => Math.max(0, c - 1));
      },
      error: () => {},
    });
  }

  iconFor(type: string): LucideIconData {
    switch (type) {
      case 'Welcome':
        return Sparkles;
      case 'Activity':
        return Target;
      case 'Project':
        return Users;
      default:
        return Info;
    }
  }

  timeAgo(iso: string): string {
    const diff = Date.now() - new Date(iso).getTime();
    const min = Math.floor(diff / 60000);
    if (min < 1) return 'ahora';
    if (min < 60) return `hace ${min} min`;
    const h = Math.floor(min / 60);
    if (h < 24) return `hace ${h} h`;
    const d = Math.floor(h / 24);
    if (d < 7) return `hace ${d} d`;
    return new Date(iso).toLocaleDateString('es-ES', { day: 'numeric', month: 'short' });
  }
}
