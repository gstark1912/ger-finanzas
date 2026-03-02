erDiagram
    expense_accounts {
        uuid id PK
        varchar name
        varchar type
        varchar currency
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    saving_accounts {
        uuid id PK
        varchar name
        varchar type
        varchar currency
        boolean is_active
        timestamp created_at
        timestamp updated_at
    }

    investment_accounts {
        uuid id PK
        varchar name
        varchar currency
        boolean is_active
        decimal expected_annual_return_pct
        timestamp created_at
        timestamp updated_at
    }

    months {
        uuid id PK
        int year
        int month_number
    }

    fx_rate_months {
        uuid id PK
        uuid month_id FK
        varchar base_currency
        varchar quote_currency
        decimal rate
    }

    fixed_expense_definitions {
        uuid id PK
        varchar name
        uuid expense_account_id FK
        varchar currency
        boolean is_active
        int? expire_day
        timestamp created_at
    }

    fixed_expense_month_entries {
        uuid id PK
        uuid fixed_expense_definition_id FK
        uuid month_id FK
        decimal amount
        timestamp paid_at
    }

    saving_account_months {
        uuid id PK
        uuid saving_account_id FK
        uuid month_id FK
        decimal balance
    }

    saving_account_month_transactions {
        uuid id PK
        uuid saving_account_month_id FK
        decimal amount
        date date
        varchar description
    }

    investment_account_months {
        uuid id PK
        uuid investment_account_id FK
        int month
        int year
        decimal balance
        decimal income
        decimal expenses
    }

    card_installments {
        uuid id PK
        uuid expense_account_id FK
        varchar description
        decimal total
        varchar currency
        int payments
        int installments
        boolean active
        date date
        int starting_month
    }

    card_expense_months {
        uuid id PK
        uuid card_installment_id FK
        decimal total
        varchar currency
        int installment
        int month
        int year
        boolean paid
    }

    card_balance_months {
        uuid id PK
        uuid expense_account_id FK
        int month
        int year
        decimal other_expenses_ars
        decimal other_expenses_usd
        boolean paid
    }

    variable_expenses {
        uuid id PK
        uuid expense_account_id FK
        decimal total
        varchar currency
        int month
        int year
    }

    months ||--|| fx_rate_months : "has one"
    expense_accounts ||--o{ fixed_expense_definitions : "has many"
    fixed_expense_definitions ||--o{ fixed_expense_month_entries : "has many"
    months ||--o{ fixed_expense_month_entries : "has many"
    saving_accounts ||--o{ saving_account_months : "has many"
    months ||--o{ saving_account_months : "has many"
    saving_account_months ||--o{ saving_account_month_transactions : "has many"
    investment_accounts ||--o{ investment_account_months : "has many"
    expense_accounts ||--o{ card_installments : "has many"
    card_installments ||--o{ card_expense_months : "has many"
    expense_accounts ||--o{ card_balance_months : "has many"
    expense_accounts ||--o{ variable_expenses : "has many"
