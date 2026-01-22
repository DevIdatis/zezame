import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FeatureSection } from './feature-section';

describe('FeatureSection', () => {
  let component: FeatureSection;
  let fixture: ComponentFixture<FeatureSection>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [FeatureSection],
    }).compileComponents();

    fixture = TestBed.createComponent(FeatureSection);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
