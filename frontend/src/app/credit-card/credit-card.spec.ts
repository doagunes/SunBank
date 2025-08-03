import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CreditCard } from './credit-card';

describe('CreditCard', () => {
  let component: CreditCard;
  let fixture: ComponentFixture<CreditCard>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CreditCard]
    })
    .compileComponents();

    fixture = TestBed.createComponent(CreditCard);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
