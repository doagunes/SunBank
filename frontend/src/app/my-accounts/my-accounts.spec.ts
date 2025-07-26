import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MyAccounts } from './my-accounts';

describe('MyAccounts', () => {
  let component: MyAccounts;
  let fixture: ComponentFixture<MyAccounts>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [MyAccounts]
    })
    .compileComponents();

    fixture = TestBed.createComponent(MyAccounts);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
