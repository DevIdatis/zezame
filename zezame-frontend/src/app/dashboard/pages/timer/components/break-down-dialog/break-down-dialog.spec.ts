import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BreakDownDialog } from './break-down-dialog';

describe('BreakDownDialog', () => {
  let component: BreakDownDialog;
  let fixture: ComponentFixture<BreakDownDialog>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BreakDownDialog],
    }).compileComponents();

    fixture = TestBed.createComponent(BreakDownDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
